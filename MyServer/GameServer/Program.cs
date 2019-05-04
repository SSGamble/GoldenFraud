using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyServer;

namespace GameServer
{
    class Program
    {
        static void Main(string[] args)
        {
            ServerPeer server = new ServerPeer();
            server.SetApplication(new NetMsgCenter()); // 设置应用层
            server.StartServer("127.0.0.1", 6666, 100);

            Database.DatabaseManager.StartConnection(); // 连接数据库

            Console.ReadKey();
        }
    }
}
