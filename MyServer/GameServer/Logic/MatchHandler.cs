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

    public class MatchHandler : IHandler {
        /// <summary>
        /// 匹配房间缓存集合，保证唯一性
        /// </summary>
        private List<MatchCache> matchCacheList = Caches.matchCacheList;

        public void Disconnect(MyServer.ClientPeer client) {

        }

        public void Receive(MyServer.ClientPeer client, int subCode, object value) {
            switch (subCode) {
                case MatchCode.Enter_CREQ: // 客户端注册请求
                    EnterRoom(client, (int)value);
                    break;
                default:
                    break;
            }
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
