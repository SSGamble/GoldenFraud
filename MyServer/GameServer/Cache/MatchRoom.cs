using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyServer;

namespace GameServer.Cache {

    /// <summary>
    /// 匹配房间
    /// </summary>
    public class MatchRoom {

        /// <summary>
        /// 房间ID，唯一标识
        /// </summary>
        public int roomId { get; private set; }

        /// <summary>
        /// 房间内的玩家
        /// </summary>
        public List<ClientPeer> clientList { get;private set; }

        /// <summary>
        /// 房间内已准备的 玩家id
        /// </summary>
        public List<int> readyUidList { get; set; }


        public MatchRoom(int id) {
            roomId = id;
            clientList = new List<ClientPeer>();
            readyUidList = new List<int>();
        }


        /// <summary>
        /// 获取房间是否满了
        /// </summary>
        /// <returns></returns>
        public bool IsFull() {
            return clientList.Count == 3;
        }

        /// <summary>
        /// 获取房间是否为空
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty() {
            return clientList.Count == 0;
        }

        /// <summary>
        /// 获取是否全部玩家都准备了，如果返回 true，就可以开始游戏了
        /// </summary>
        /// <returns></returns>
        public bool IsAllReady() {
            return readyUidList.Count == 3;
        }

        /// <summary>
        /// 进入房间
        /// </summary>
        /// <param name="client"></param>
        public void Enter(ClientPeer client) {
            clientList.Add(client);
        }

        /// <summary>
        /// 离开房间
        /// </summary>
        /// <param name="client"></param>
        public void Leave(ClientPeer client) {
            clientList.Remove(client);
            if (readyUidList.Contains(client.Id)) { // 从准备列表移除
                readyUidList.Remove(client.Id);
            }
        }

        /// <summary>
        /// 准备
        /// </summary>
        /// <param name="userId"></param>
        public void Ready(int userId) {
            readyUidList.Add(userId);
        }

        /// <summary>
        /// 取消准备
        /// </summary>
        /// <param name="userId"></param>
        public void UnReady(int userId) {
            if (readyUidList.Contains(userId)) { 
                readyUidList.Remove(userId);
            }
        }

        /// <summary>
        /// 广播发消息，给除自己以外的客户端发消息
        /// </summary>
        public void Broadcast(int opCode,int subCode,object value,ClientPeer exceptClient = null) {
            NetMsg msg = new NetMsg(opCode, subCode, value);
            byte[] data = EncodeTool.EncodeMsg(msg);
            byte[] packet = EncodeTool.EncodePacket(data);
            foreach (var client in clientList) {
                if (client == exceptClient) {
                    continue;
                }
                client.SendMsg(packet);
            }
        }
    }
}
