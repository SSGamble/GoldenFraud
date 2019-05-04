using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace MyServer 
{
    /// <summary>
    /// 客户端对象的连接池
    /// </summary>
    public class ClientPeerPool
    {
        // 连接池 队列
        private Queue<ClientPeer> clientPeerQueue;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="maxCount">最大连接数量</param>
        public ClientPeerPool(int maxCount) {
            clientPeerQueue = new Queue<ClientPeer>(maxCount);
        }

        /// <summary>
        /// 入队列
        /// </summary>
        /// <param name="client"></param>
        public void Enqueue(ClientPeer client) {
            clientPeerQueue.Enqueue(client);
        }

        /// <summary>
        /// 出队列
        /// </summary>
        /// <returns></returns>
        public ClientPeer Dequeue() {
            return clientPeerQueue.Dequeue();
        }
    }
}