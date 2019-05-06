using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameServer.Cache;
using GameServer.Database;
using MyServer;
using Protocol.Code;
using Protocol.Dto;

namespace GameServer.Logic {

    /// <summary>
    /// 开始游戏的委托
    /// </summary>
    /// <param name="clientList"></param>
    /// <param name="roomType"></param>
    public delegate void StartFight(List<ClientPeer> clientList, int roomType);

    public class MatchHandler : IHandler {

        public StartFight startFight;
        /// <summary>
        /// 匹配房间缓存集合，保证唯一性
        /// </summary>
        private List<MatchCache> matchCacheList = Caches.matchCacheList;

        /// <summary>
        /// 断开连接
        /// </summary>
        /// <param name="client"></param>
        public void Disconnect(MyServer.ClientPeer client) {
            for (int i = 0; i < matchCacheList.Count; i++)
            {
                LeaveRoom(client, i);
            }
        }

        public void Receive(MyServer.ClientPeer client, int subCode, object value) {
            switch (subCode) {
                case MatchCode.Enter_CREQ: // 客户端注册请求
                    EnterRoom(client, (int)value);
                    break;
                case MatchCode.Leave_CREQ:
                    LeaveRoom(client, (int)value);
                    break;
                case MatchCode.Ready_CREQ:
                    Ready(client, (int)value);
                    break;
                case MatchCode.UnReady_CREQ:
                    UnReady(client, (int)value);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 取消准备的请求
        /// </summary>
        private void UnReady(ClientPeer client, int roomType)
        {
            SingleExecute.Instance.Exeecute(() => {
                // 不在匹配房间
                if (matchCacheList[roomType].IsMatching(client.Id) == false)
                {
                    return;
                }
                MatchRoom room = matchCacheList[roomType].GetRoom(client.Id);
                room.UnReady(client.Id);
                // 广播给房间内的其他玩家
                room.Broadcast(OpCode.Match, MatchCode.UnReady_BRO, client.Id);
            });
        }

        /// <summary>
        /// 客户端发来准备请求
        /// </summary>
        private void Ready(ClientPeer client, int roomType)
        {
            SingleExecute.Instance.Exeecute(() => {
                // 不在匹配房间
                if (matchCacheList[roomType].IsMatching(client.Id) == false)
                {
                    return;
                }
                MatchRoom room = matchCacheList[roomType].GetRoom(client.Id);
                room.Ready(client.Id);
                // 广播给房间内的其他玩家
                room.Broadcast(OpCode.Match, MatchCode.Ready_BRO, client.Id);

                // 全部都准备了，可以开始游戏了
                if (room.IsAllReady())
                {
                    startFight(room.clientList, roomType);
                    // 通知房间里的所有玩家，开始游戏了
                    room.Broadcast(OpCode.Match, MatchCode.StartGame_BRO, null);
                    // 销毁房间
                    matchCacheList[roomType].DesRoom(room);
                }
            });
        }

        /// <summary>
        /// 客户端离开房间的请求
        /// </summary>
        /// <param name="client"></param>
        /// <param name="roomType"></param>
        private void  LeaveRoom(ClientPeer client,int roomType)
        {
            SingleExecute.Instance.Exeecute(() => {
                // 不在匹配房间
                if (matchCacheList[roomType].IsMatching(client.Id) == false)
                {
                    return;
                }
                MatchRoom room = matchCacheList[roomType].Leave(client.Id);
                // 广播给房间内的其他玩家
                room.Broadcast(OpCode.Match, MatchCode.Leave_BRO,  client.Id);
            });
        }

        /// <summary>
        /// 客户端进入房间的请求
        /// </summary>
        /// <param name="client"></param>
        /// <param name="roomType"></param>
        private void EnterRoom(ClientPeer client, int roomType) {
            SingleExecute.Instance.Exeecute(() => {
                // 判断当前的客户端连接对象是不是在匹配房间里面，如果在，忽略
                if (matchCacheList[roomType].IsMatching(client.Id)) {
                    return;
                }
                MatchRoom room = matchCacheList[roomType].Enter(client);
                // 构造 UserDto
                UserDto userDto = DatabaseManager.CreateUserDto(client.Id);
                // 广播给房间内的所有玩家，除了自身，有新的玩家进来了，参数：新进用户的 UserDto
                room.Broadcast(OpCode.Match, MatchCode.Enter_BRO, userDto, client);

                // 给客户端一个相应，参数：房间传输模型，包含房间内的正在等待的玩家以及准备的玩家id 集合
                client.SendMsg(OpCode.Match, MatchCode.Enter_SRES, MakeMatchRoomDto(room));
                if (roomType == 0) {
                    Console.WriteLine(userDto.name + "进入底注为 10 的房间");
                }
                if (roomType == 1) {
                    Console.WriteLine(userDto.name + "进入底注为 20 的房间");
                }
                if (roomType == 2) {
                    Console.WriteLine(userDto.name + "进入底注为 50 的房间");
                }
            });
        }

        /// <summary>
        /// 构造 匹配房间传输模型
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        private MatchRoomDto MakeMatchRoomDto(MatchRoom room) {
            MatchRoomDto dto = new MatchRoomDto();
            for (int i = 0; i < room.clientList.Count; i++) {
                dto.Enter(DatabaseManager.CreateUserDto(room.clientList[i].Id));
            }
            dto.readyUserList = room.readyUidList;
            return dto;
        }
    }
}
