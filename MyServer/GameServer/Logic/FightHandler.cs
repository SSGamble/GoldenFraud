using GameServer.Cache;
using GameServer.Cache.Fight;
using MyServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameServer.Logic
{
    public class FightHandler:IHandler
    {
        private FightCache fightCache = Caches.FightCache;

        public void Disconnect(MyServer.ClientPeer client)
        {

        }

        public void Receive(MyServer.ClientPeer client, int subCode, object value)
        {

        }

        /// <summary>
        /// 开始战斗
        /// </summary>
        /// <param name="clientList"></param>
        /// <param name="roomType"></param>
        public void StartFight(List<ClientPeer> clientList,int roomType)
        {
            SingleExecute.Instance.Exeecute(() => {
                FightRoom room = fightCache.CreateRoom(clientList);
                switch (roomType)
                {
                    case 0:
                        room.bottomStakes = 10;
                        room.topStakes = 100;
                        room.lastPlayerStakes = 10;
                        break;
                    case 1:
                        room.bottomStakes = 20;
                        room.topStakes = 200;
                        room.lastPlayerStakes = 20;
                        break;
                    case 2:
                        room.bottomStakes = 50;
                        room.topStakes = 500;
                        room.lastPlayerStakes = 50;
                        break;
                    default:
                        break;
                }

                // 选择庄家
                ClientPeer bankerClient = room.SetBanker();
                // 发牌
                room.DealCard();
                // 对手牌排序
                room.SortAllPlayerCard();
                // 获得牌型
                room.GetAllPlayerCardType();

                
            });
        }
    }
}
