using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace MyServer {

    /// <summary>
    /// 服务器
    /// </summary>
    public class ServerPeer {

        /// <summary>
        /// 服务器端 Socket
        /// </summary>
        private Socket serverSocket;
        /// <summary>
        /// 信号计量器，用于线程的拥塞控制，限制可同时访问某一资源或资源池的线程数。
        /// </summary>
        private Semaphore semaphore;
        /// <summary>
        /// 客户端对象连接池
        /// </summary>
        private ClientPeerPool clientPeerPool;
        /// <summary>
        /// 应用层
        /// </summary>
        private IApplication application;
        /// <summary>
        /// 设置应用层
        /// </summary>
        /// <param name="application"></param>
        public void SetApplication(IApplication application) {
            this.application = application;
        }


        /// <summary>
        /// 开启服务器
        /// </summary>
        public void StartServer(string ip, int port, int maxClient) {
            try {
                clientPeerPool = new ClientPeerPool(maxClient); // 客户端连接池实例
                // 初始化 Semaphore 类的新实例，并指定初始入口数和最大并发入口数
                semaphore = new Semaphore(maxClient, maxClient);
                // 创建 Socket 对象
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // 填满客户端对象连接池，需要的时候再去取
                for (int i = 0; i < maxClient; i++) {
                    ClientPeer temp = new ClientPeer();
                    temp.receiveCompleted = ReceiveProcessCompleted; // 消息处理完成后的委托，注册函数
                    temp.ReceiveArgs.Completed += ReceiveArgs_Completed; // 注册事件
                    clientPeerPool.Enqueue(temp);
                }

                // 绑定
                serverSocket.Bind(new IPEndPoint(IPAddress.Parse(ip), port));
                // 监听该接口上的请求
                serverSocket.Listen(maxClient);
                Console.WriteLine("服务器启动成功");

                // 连接
                StartAccept(null);
            }
            catch (Exception e) {

                Console.WriteLine(e.Message);
            }
        }

        #region 接收客户端的连接请求

        /// <summary>
        /// 开始接收客户端的连接
        ///     SocketAsyncEventArgs: 异步套接字的操作对象
        /// </summary>
        private void StartAccept(SocketAsyncEventArgs e) {
            if (e == null) {
                e = new SocketAsyncEventArgs();
                e.Completed += e_Completed; // 注册事件
            }

            // true：代表正在接收连接，连接成功后会触发 Completed 事件，false：接收成功了
            bool result = serverSocket.AcceptAsync(e); // 异步的方式

            // 接收成功
            if (result == false) {
                ProcessAccept(e);
            }
        }

        /// <summary>
        /// 异步接收客户端的连接成功后会被调用
        /// </summary>
        void e_Completed(object sender, SocketAsyncEventArgs e) {
            ProcessAccept(e);
        }

        /// <summary>
        /// 处理连接请求
        /// </summary>
        private void ProcessAccept(SocketAsyncEventArgs e) {
            // 阻止当前线程，直到当前 WaitHandle 收到信号
            semaphore.WaitOne();
            // 获取连接的客户端套接字
            // ClientPeer client = new ClientPeer(); // 每连接一个就 new 一个会损耗性能，所以用客户端对象连接池来解决
            ClientPeer client = clientPeerPool.Dequeue();
            client.ClientSocket = e.AcceptSocket;
            Console.WriteLine(client.ClientSocket.RemoteEndPoint + "客户端连接成功");
            // 接收消息
            StartRecive(client);

            e.AcceptSocket = null;
            StartAccept(e); // 循环接收客户端的连接
        }

        #endregion

        #region 接收数据
        /// <summary>
        /// 开始接收数据
        /// </summary>
        private void StartRecive(ClientPeer client) {
            try {
                bool result = client.ClientSocket.ReceiveAsync(client.ReceiveArgs);
                if (result == false) {
                    ProcessReceive(client.ReceiveArgs);
                }
            }
            catch (Exception e) {

                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// 异步接收数据完成后的调用
        /// </summary>
        private void ReceiveArgs_Completed(object sender, SocketAsyncEventArgs e) {
            ProcessReceive(e);
        }

        /// <summary>
        /// 处理数据的接收
        /// </summary>
        private void ProcessReceive(SocketAsyncEventArgs e) {
            // e.UserToken 用于获取或设置与此异步套接字操作关联的用户或应用程序对象，即 ClientPeer
            ClientPeer client = e.UserToken as ClientPeer;
            // 判断数据是否接收成功
            if (client.ReceiveArgs.SocketError == SocketError.Success && client.ReceiveArgs.BytesTransferred > 0) {
                // 将接收的数据拷贝到 packet 里面
                int length = client.ReceiveArgs.BytesTransferred; // 接收到的数据的长度
                byte[] packet = new byte[length];
                Buffer.BlockCopy(client.ReceiveArgs.Buffer, 0, packet, 0, length);

                // 让 ClientPeer 自身处理接收到的数据
                client.ProcessReceive(packet);

                // 伪递归
                StartRecive(client);
            }
            else { // 数据接收不成功，断开连接
                // 没有传输的字节数，就代表断开了连接，有两种可能
                if (client.ReceiveArgs.BytesTransferred == 0) {
                    // 客户端主动断开连接
                    if (client.ReceiveArgs.SocketError == SocketError.Success) {
                        Disconnect(client, "客户端主动断开连接");
                    }
                    // 因为网络异常，被动断开连接
                    else {
                        Disconnect(client, client.ReceiveArgs.SocketError.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// 一条消息处理完成后的回调
        /// </summary>
        /// <param name="client"></param>
        /// <param name="msg"></param>
        private void ReceiveProcessCompleted(ClientPeer client,NetMsg msg) {
            // 交给应用层处理这个消息
            application.Receive(client, msg);
        }

        #endregion

        #region 断开连接
        /// <summary>
        /// 客户端断开连接
        /// </summary>
        /// <param name="client"></param>
        /// <param name="reason"></param>
        private void Disconnect(ClientPeer client, string reason) {
            try {
                if (client == null) {
                    throw new Exception("客户端为空，无法断开连接");

                }
                Console.WriteLine(client.ClientSocket.RemoteEndPoint + "客户端断开连接，原因" + reason);
                application.Disconnect(client); // 应用层断开连接
                client.Disconnect(); // 让客户端处理断开连接
                clientPeerPool.Enqueue(client); // 回收
                semaphore.Release(); // 退出信号量并返回前一个计数
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
            }
        }
        #endregion
    }
}
