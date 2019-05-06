using MyServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Cache.Fight
{
    /// <summary>
    /// 战斗缓存
    /// </summary>
    public class FightCache
    {
        /// <summary>
        /// 玩家Id 和 战斗房间id 的映射
        /// </summary>
        public Dictionary<int, int> userIdRoomIdDic = new Dictionary<int, int>();
        /// <summary>
        /// 战斗房间id 和 战斗房间 的映射
        /// </summary>
        public Dictionary<int, FightRoom> roomIdModelDic = new Dictionary<int, FightRoom>();
        /// <summary>
        /// 战斗房间重用列表
        /// </summary>
        public Queue<FightRoom> roomQueue = new Queue<FightRoom>();
        /// <summary>
        /// 线程安全的整型，防止多个用户访问
        /// </summary>
        private ThreadSafeInt roomId = new ThreadSafeInt(-1); // 因为 value++，-1+1=0

        /// <summary>
        /// 创建房间
        /// </summary>
        public FightRoom CreateRoom(List<ClientPeer> clientList)
        {
            FightRoom room = null;
            // 优先从重用队列里取
            if (roomQueue.Count>0)
            {
                room = roomQueue.Dequeue();
                room.Init(clientList);
            }
            // 如果没有就自己开一间
            else 
            {
                room = new FightRoom(roomId.AddGet(), clientList); // 线程安全的整形
            }
            foreach (var client in clientList)
            {
                userIdRoomIdDic.Add(client.Id, room.roomId);
            }
            roomIdModelDic.Add(room.roomId, room);
            return room;
        }

        /// <summary>
        /// 销毁房间
        /// </summary>
        /// <param name="room"></param>
        public void DesRoom(FightRoom room)
        {
            roomIdModelDic.Remove(room.roomId);
            // 移除玩家
            foreach (var player in room.playerList)
            {
                userIdRoomIdDic.Remove(player.id);
            }
            // 初始化数据
            room.Destory();
            // 加入重用队列
            roomQueue.Enqueue(room);
        }

        /// <summary>
        /// 判断用户是否在战斗房间
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool IsFighting(int userId)
        {
            return userIdRoomIdDic.ContainsKey(userId);
        }

        /// <summary>
        /// 通过用户id 获取战斗房间
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public FightRoom GetFightRoomByUserId(int userId)
        {
            int roomId = userIdRoomIdDic[userId];
            return roomIdModelDic[roomId];
        }
    }

}
