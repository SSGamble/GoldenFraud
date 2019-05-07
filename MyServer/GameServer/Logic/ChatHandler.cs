using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameServer.Cache;
using GameServer.Cache.Fight;
using MyServer;
using Protocol.Code;
using Protocol.Dto.Fight;

namespace GameServer.Logic
{
    public class ChatHandler:IHandler
    {
        /// <summary>
        /// 匹配房间缓存
        /// </summary>
        private List<MatchCache> matchCaches = Caches.matchCacheList;
        /// <summary>
        /// 战斗缓存
        /// </summary>
        private FightCache fightCache = Caches.FightCache;
        /// <summary>
        /// 聊天传输模型
        /// </summary>
        private ChatDto chatDto = new ChatDto();

        public void Disconnect(MyServer.ClientPeer client) {

        }

        public void Receive(MyServer.ClientPeer client, int subCode, object value) {
            switch (subCode) {
                case ChatCode.CREQ:
                    Chat(client, value.ToString());
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 客户端聊天的请求处理
        /// </summary>
        /// <param name="client"></param>
        /// <param name="msg"></param>
        private void Chat(ClientPeer client, string msg) {
            SingleExecute.Instance.Exeecute(() => {
                foreach (var matchCache in matchCaches) {
                    if (matchCache.IsMatching(client.Id)) { // 客户端在匹配房间里
                        MatchRoom room = matchCache.GetRoom(client.Id);
                        chatDto.Change(client.Id, client.UserName, msg);
                        room.Broadcast(OpCode.Chat, ChatCode.BRO, chatDto);
                    }
                }
                if (fightCache.IsFighting(client.Id) == false) return;

                FightRoom fightRoom = fightCache.GetFightRoomByUserId(client.Id);
                chatDto.Change(client.Id, client.UserName, msg);
                fightRoom.Broadcast(OpCode.Chat, ChatCode.BRO, chatDto);
            });
        }
    }
}
