using GameServer.Cache;
using GameServer.Cache.Fight;
using GameServer.Database;
using MyServer;
using MyServer.TimerTool;
using Protocol.Code;
using Protocol.Dto;
using Protocol.Dto.Fight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameServer.Logic {
    public class FightHandler : IHandler {
        /// <summary>
        /// 战斗缓存
        /// </summary>
        private FightCache fightCache = Caches.FightCache;
        /// <summary>
        /// 下注的传输模型
        /// </summary>
        private StakesDto stakesDto = new StakesDto();

        public void Disconnect(MyServer.ClientPeer client) {
            LeaveRoom(client);
        }

        public void Receive(MyServer.ClientPeer client, int subCode, object value) {
            switch (subCode) {
                case FightCode.Leave_CREQ:
                    LeaveRoom(client);
                    break;
                case FightCode.LookCard_CREQ:
                    LookCard(client);
                    break;
                case FightCode.Follow_CREQ:
                    Follow(client);
                    break;
                case FightCode.AddStakes_CREQ:
                    AddStakes(client, (int)value);
                    break;
                case FightCode.GiveUpCard_CREQ:
                    GiveUpCard(client);
                    break;
                case FightCode.CompareCard_CREQ:
                    CompareCard(client, (int)value);
                    break;
                default:
                    break;
            }
        }

        private void CompareCard(ClientPeer client, int value) {
            throw new NotImplementedException();
        }


        /// <summary>
        /// 客户端弃牌的请求处理
        /// </summary>
        /// <param name="client"></param>
        private void GiveUpCard(ClientPeer client) {
            SingleExecute.Instance.Exeecute(() => {
                if (fightCache.IsFighting(client.Id) == false) return;

                FightRoom room = fightCache.GetFightRoomByUserId(client.Id);
                room.giveUpUserIdList.Add(client.Id);
                room.Broadcast(OpCode.Fight, FightCode.GiveUpCard_BRO, client.Id);

                //离开的玩家是本次下注的玩家,这样的需转换下一个玩家下注
                if (room.roundModel.CurrentStakesUserId == client.Id) {
                    //轮换下注
                    Turn(client);
                }
                if (room.giveUpUserIdList.Count >= 1 && room.leaveUserIdList.Count >= 1) {
                    GameOver(room);
                }
                //游戏结束
                if (room.giveUpUserIdList.Count == 2) {
                    GameOver(room);
                }
            });
        }

        private void GiveUpCard(int userId) {
            SingleExecute.Instance.Exeecute(() => {
                if (fightCache.IsFighting(userId) == false) return;

                FightRoom room = fightCache.GetFightRoomByUserId(userId);
                room.giveUpUserIdList.Add(userId);
                room.Broadcast(OpCode.Fight, FightCode.GiveUpCard_BRO, userId);

                if (room.giveUpUserIdList.Count >= 1 && room.leaveUserIdList.Count >= 1) {
                    GameOver(room);
                }
                //游戏结束
                if (room.giveUpUserIdList.Count == 2) {
                    GameOver(room);
                }
            });
        }

        /// <summary>
        /// 游戏结束
        /// </summary>
        /// <param name="room"></param>
        private void GameOver(FightRoom room) {
            PlayerDto winPlayer = null;
            List<PlayerDto> loseList = new List<PlayerDto>();

            foreach (var player in room.playerList) {
                //胜利的玩家
                if (!room.IsGiveUpCard(player.id) && !room.IsLeaveRoom(player.id)) {
                    winPlayer = player;
                }
                else {
                    loseList.Add(player);
                }
            }
            DatabaseManager.UpdateCoin(winPlayer.id, room.stakesSum);
            overDto.Change(winPlayer, loseList, room.stakesSum);
            room.Broadcast(OpCode.Fight, FightCode.GameOver_BRO, overDto);
            //销毁房间
            fightCache.DestoryRoom(room);
            //清空定时任务
            TimerManager.Instance.Clear();
        }

        /// <summary>
        /// 客户端的加注处理
        /// </summary>
        /// <param name="client"></param>
        /// <param name="value"></param>
        private void AddStakes(ClientPeer client, int multiple) {
            SingleExecute.Instance.Exeecute(() => {
                if (fightCache.IsFighting(client.Id) == false) return;

                FightRoom room = fightCache.GetFightRoomByUserId(client.Id);
                room.lastPlayerStakes *= multiple;
                if (room.lastPlayerStakes > room.topStakes) { // 顶注
                    room.lastPlayerStakes = room.topStakes;
                }
                room.stakesSum += room.lastPlayerStakes;
                int stakesSum = room.UpdatePlayerStakesSum(client.Id, room.lastPlayerStakes);
                int remainCoin = DatabaseManager.UpdateCoin(client.Id, -room.lastPlayerStakes);

                stakesDto.Change(client.Id, remainCoin, room.lastPlayerStakes, stakesSum, StakesDto.StakesType.NoLook);
                room.Broadcast(OpCode.Fight, FightCode.PutStakes_BRO, stakesDto);
                // 剩余金币不够
                if (remainCoin < room.lastPlayerStakes) {
                    GiveUpCard(client);
                    return;
                }

                Turn(client);
            });
        }

        /// <summary>
        /// 客户端跟注请求的处理
        /// </summary>
        /// <param name="client"></param>
        private void Follow(ClientPeer client) {
            SingleExecute.Instance.Exeecute(() => {
                if (fightCache.IsFighting(client.Id) == false) {
                    return;
                }
                FightRoom room = fightCache.GetFightRoomByUserId(client.Id);
                room.stakesSum += room.lastPlayerStakes;
                int stakesSun = room.UpdatePlayerStakesSum(client.Id, room.lastPlayerStakes);
                int remainCoin = DatabaseManager.UpdateCoin(client.Id, -(room.lastPlayerStakes));
                stakesDto.Change(client.Id, remainCoin, room.lastPlayerStakes, stakesSun, StakesDto.StakesType.NoLook);
                room.Broadcast(OpCode.Fight, FightCode.PutStakes_BRO, stakesDto);
                // 剩余金币不够
                if (remainCoin < room.lastPlayerStakes) {
                    GiveUpCard(client);
                    return;
                }
                // 轮到下一个玩家下注
                Turn(client);
            });
        }

        /// <summary>
        /// 客户端看牌请求
        /// </summary>
        /// <param name="client"></param>
        private void LookCard(ClientPeer client) {
            SingleExecute.Instance.Exeecute(() => {
                if (fightCache.IsFighting(client.Id) == false) {
                    return;
                }
                FightRoom room = fightCache.GetFightRoomByUserId(client.Id);
                room.Broadcast(OpCode.Fight, FightCode.LookCard_BRO, client.Id);
            });
        }

        private ClientPeer timerClient;

        /// <summary>
        /// 轮换下注
        /// </summary>
        /// <param name="client"></param>
        private void Turn(ClientPeer client) {
            // 清空计时器任务
            TimerManager.Instance.Clear();
            if (fightCache.IsFighting(client.Id) == false) {
                return;
            }
            FightRoom room = fightCache.GetFightRoomByUserId(client.Id);
            int nextID = room.Turn();
            if (room.IsGiveUpCard(nextID) || room.IsLeaveRoom(nextID)) {
                // 如果下一位玩家离开或弃牌了，就继续轮换下注，直到改玩家不离开也不弃牌为止
                Turn(client);
            }
            else {
                timerClient = DatabaseManager.GetClientPeerByUserId(nextID);
                // 添加计时器任务
                TimerManager.Instance.AddTimerEvent(10, TimerDelegate);
                Console.WriteLine("当前下注者" + client.UserName);
                room.Broadcast(OpCode.Fight, FightCode.StartStakes_BRO, nextID);
            }
        }

        /// <summary>
        /// 计时器要执行的任务
        /// </summary>
        private void TimerDelegate() {
            // 跟注
            Follow(timerClient);
        }

        /// <summary>
        /// 客户端离开请求的处理
        /// </summary>
        /// <param name="client"></param>
        private void LeaveRoom(ClientPeer client) {
            SingleExecute.Instance.Exeecute(() => {
                // 不在战斗房间
                if (fightCache.IsFighting(client.Id) == false) {
                    return;
                }
                FightRoom room = fightCache.GetFightRoomByUserId(client.Id);
                room.leaveUserIdList.Add(client.Id);

                // 离开房间的惩罚
                DatabaseManager.UpdateCoin(client.Id, -(room.bottomStakes * 20));
                room.Broadcast(OpCode.Fight, FightCode.Leave_BRO, client.Id);

                // 离开的玩家是本次下注的玩家，这样的话需要转换下一个玩家下注
                if (room.roundModel.CurrentStakesUserId == client.Id) {
                    // 轮换下注
                    Turn(client);
                }
                // 游戏结束
                if (room.leaveUserIdList.Count == 2) {
                    return;
                }
                // 销毁房间
                if (room.leaveUserIdList.Count == 3) {
                    fightCache.DesRoom(room);
                }
            });
        }

        /// <summary>
        /// 开始战斗
        /// </summary>
        /// <param name="clientList"></param>
        /// <param name="roomType"></param>
        public void StartFight(List<ClientPeer> clientList, int roomType) {
            SingleExecute.Instance.Exeecute(() => {
                FightRoom room = fightCache.CreateRoom(clientList);
                switch (roomType) {
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
                foreach (var client in clientList) {
                    room.UpdatePlayerStakesSum(client.Id, room.bottomStakes);
                }
                // 选择庄家
                ClientPeer bankerClient = room.SetBanker();
                // 重置位置，排序
                room.ResetPosition(bankerClient.Id);
                // 发牌
                room.DealCard();
                // 对手牌排序
                room.SortAllPlayerCard();
                // 获得牌型
                room.GetAllPlayerCardType();
                room.Broadcast(OpCode.Fight, FightCode.StartFight_BRO, room.playerList);
                // 转换下注，换到庄家的下一位玩家下注
                Turn(bankerClient);
            });
        }
    }
}
