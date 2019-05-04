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
    /// 
    /// </summary>
    public interface IApplication
    {
        /// <summary>
        /// 断开连接
        /// </summary>
        /// <param name="client"></param>
        void Disconnect(ClientPeer client);
        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="client"></param>
        /// <param name="msg"></param>
        void Receive(ClientPeer client, NetMsg msg);
    }
}