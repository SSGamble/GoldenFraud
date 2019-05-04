using MyServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameServer.Logic
{
    public interface IHandler
    {
        /// <summary>
        /// 断开连接
        /// </summary>
        void Disconnect(ClientPeer client);
        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="client">客户端连接对象</param>
        /// <param name="subCode">子操作码</param>
        /// <param name="value">参数</param>
        void Receive(ClientPeer client, int subCode,object value);
    }
}
