using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace MyServer {
    /// <summary>
    /// 客户端
    /// </summary>
    public class ClientPeer {

        public int Id { get; set; }
        public string UserName { get; set; }

        public Socket ClientSocket { get; set; }
        private NetMsg msg;

        public ClientPeer() {
            msg = new NetMsg();
            ReceiveArgs = new SocketAsyncEventArgs();
            ReceiveArgs.UserToken = this; // 赋值，用于服务器端的获取
            ReceiveArgs.SetBuffer(new byte[2048], 0, 2048); // 设置数据缓冲区
        }

        #region 接收数据

        /// <summary>
        /// 接收的异步套接字操作
        /// </summary>
        public SocketAsyncEventArgs ReceiveArgs { get; set; }
        /// <summary>
        /// 接收到消息之后，存放到数据缓存区
        /// </summary>
        private List<byte> cache = new List<byte>();
        /// <summary>
        /// 是否正在处理接收的数据
        /// </summary>
        private bool isProcessingReceive = false;
        /// <summary>
        /// 消息处理完成后的委托
        /// </summary>
        public delegate void ReceiveCompleted(ClientPeer client, NetMsg msg);
        public ReceiveCompleted receiveCompleted;

        /// <summary>
        /// 处理接收到的数据
        /// </summary>
        public void ProcessReceive(byte[] packet) {
            cache.AddRange(packet);
            if (isProcessingReceive == false) {
                ProcessData();
            }
        }

        /// <summary>
        /// 处理数据
        /// </summary>
        private void ProcessData() {
            isProcessingReceive = true;
            // 解析包，从缓冲区里取出一个完整的包
            byte[] packet = EncodeTool.DecodePacket(ref cache);
            if (packet == null) {
                isProcessingReceive = false;
                return;
            }
            // 反序列化
            NetMsg msg = EncodeTool.DecodeMsg(packet);

            if (receiveCompleted != null) { // 有人注册了委托
                receiveCompleted(this, msg); // 将处理完后的 msg 回调给上层
            }
            ProcessData(); // 递归，知道缓存区的数据为 null 
        }

        #endregion

        #region 发送消息
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="opCode">操作码</param>
        /// <param name="subCode">子操作码</param>
        /// <param name="value">参数</param>
        public void SendMsg(int opCode, int subCode, object value) {
            msg.Change(opCode, subCode, value);
            byte[] data = EncodeTool.EncodeMsg(msg); // 对象 转 字节数组
            byte[] packet = EncodeTool.EncodePacket(data); // 构造包头包尾
            SendMsg(packet);
        }

        public void SendMsg(byte[] packet) {
            try {
                ClientSocket.Send(packet);
            }
            catch (Exception e) {

                Console.WriteLine(e.Message);
            }
        }
        #endregion

        #region 断开连接

        /// <summary>
        /// 断开连接
        /// </summary>
        public void Disconnect() {
            cache.Clear(); // 清空缓冲区
            isProcessingReceive = false;
            ClientSocket.Shutdown(SocketShutdown.Both); // 禁用发送和接收
            ClientSocket.Close();
            ClientSocket = null;
        }

        #endregion
    }
}