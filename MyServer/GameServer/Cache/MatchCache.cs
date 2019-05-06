using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServer.Database;
using MyServer;

namespace GameServer.Cache {

    /// <summary>
    /// 匹配缓存层
    /// </summary>
    public class MatchCache {

        /// <summary>
        /// 当前正在匹配的 用户id 和 房间id 的映射
        /// </summary>
        public Dictionary<int, int> userIdRoomIdDic = new Dictionary<int, int>();

        /// <summary>
        /// 正在匹配的 房间id 和 与之对应的房间数据模型 之间的映射字典
        /// </summary>
        public Dictionary<int, MatchRoom> roomIdModelDic = new Dictionary<int, MatchRoom>();

        /// <summary>
        /// 重用房间队列
        /// </summary>
        public Queue<MatchRoom> roomQueue = new Queue<MatchRoom>();

        /// <summary>
        /// 线程安全的整型，防止多个用户访问
        /// </summary>
        private ThreadSafeInt roomId = new ThreadSafeInt(-1); // 因为 value++，-1+1=0

        /// <summary>
        /// 进入匹配房间
        /// </summary>
        public MatchRoom Enter(ClientPeer client) {
            // 先遍历正在匹配的房间数据模型字典中有没有未满的房间，如果有，加进去
            foreach (var matchRoom in roomIdModelDic.Values) {
                if (matchRoom.IsFull()) { // 满了就换一个房间
                    continue;
                }
                matchRoom.Enter(client);
                userIdRoomIdDic.Add(client.Id, matchRoom.roomId);
                return matchRoom;
            }
            // 如果执行到这里，代表正在匹配的房间数据模型字典中没有空位了，自己开一间
            MatchRoom room = null;
            if (roomQueue.Count > 0) { // 优先从重用房间队列里取，如果没有再 new 一个
                room = roomQueue.Dequeue();
            }
            else {
                room = new MatchRoom(roomId.AddGet()); // 防止 id 重复
            }
            roomIdModelDic.Add(room.roomId, room);
            userIdRoomIdDic.Add(client.Id, room.roomId);
            return room;
        }

        /// <summary>
        /// 离开匹配房间
        /// </summary>
        public MatchRoom Leave(int userId) {
            int roomId = userIdRoomIdDic[userId];
            MatchRoom room = roomIdModelDic[roomId];
            room.Leave(DatabaseManager.GetClientPeerByUserId(userId));
            userIdRoomIdDic.Remove(userId);

            // 如果房间为空，从正在匹配的房间字典中移除，并将房间加入到房间重用队列
            if (room.IsEmpty()) {
                roomIdModelDic.Remove(roomId);
                roomQueue.Enqueue(room);
            }
            return room;
        }

        /// <summary>
        /// 用户是否在匹配房间里
        /// </summary>
        public bool IsMatching(int userId) {
            return userIdRoomIdDic.ContainsKey(userId);
        }

        /// <summary>
        /// 获取玩家所在的房间
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public MatchRoom GetRoom(int userId) {
            int roomId = userIdRoomIdDic[userId];
            return roomIdModelDic[roomId];
        }

        /// <summary>
        /// 销毁房间，游戏开始时调用
        /// </summary>
        public void DesRoom(MatchRoom room){
            roomIdModelDic.Remove(room.roomId);
            // 移除 用户/房间 的映射
            foreach (var client in room.clientList) {
                userIdRoomIdDic.Remove(client.Id);
            }
            room.clientList.Clear();
            room.readyUidList.Clear();
            roomQueue.Enqueue(room); // 加入房间重用队列
        }

    }
}
