using GameServer.Cache;
using GameServer.Cache.Fight;
using GameServer.Database;
using MyServer;
using MyServer.TimerTool;
using Protocol.Code;
using Protocol.Constant;
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
        /// <summary>
        /// 比牌结果传输模型
        /// </summary>
        private CompareResultDto resultDto = new CompareResultDto();
        /// <summary>
        /// 游戏结束传输模型
        /// </summary>
        private GameOverDto gameOverDto = new GameOverDto();

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

        /// <summary>
        /// 客户端比牌的处理
        /// </summary>
        /// <param name="compareClient"></param>
        /// <param name="comparedID"></param>
        private void CompareCard(ClientPeer compareClient, int comparedID) {
            SingleExecute.Instance.Exeecute(() => {
                if (fightCache.IsFighting(compareClient.Id) == false) return;

                FightRoom room = fightCache.GetFightRoomByUserId(compareClient.Id);
                room.stakesSum += room.lastPlayerStakes;
                int stakesSum = room.UpdatePlayerStakesSum(compareClient.Id, room.lastPlayerStakes);
                int remainCoin = DatabaseManager.UpdateCoin(compareClient.Id, -room.lastPlayerStakes);
                stakesDto.Change(compareClient.Id, remainCoin, room.lastPlayerStakes, stakesSum, StakesDto.StakesType.Look);
                room.Broadcast(OpCode.Fight, FightCode.PutStakes_BRO, stakesDto);

                // 拿到 3 个玩家的 DTO
                PlayerDto c1Dto = null, c2Dto = null, otherDto = null;
                foreach (var player in room.playerList) {
                    if (player.id == compareClient.Id) {
                        c1Dto = player;
                    }
                    else if (player.id == comparedID) {
                        c2Dto = player;
                    }
                    else {
                        otherDto = player;
                    }
                }
                ClientPeer otherClient = DatabaseManager.GetClientPeerByUserId(otherDto.id);
                // 比牌
                CompareCard(room,compareClient,c1Dto,c2Dto, otherClient);
            });
        }

        /// <summary>
        /// 比牌结果
        /// </summary>
        private void CompareResult(FightRoom room,ClientPeer c1,PlayerDto winDto,PlayerDto loseDto,ClientPeer otherClient) {
            // 转到胜利的玩家下注
            Turn(c1,winDto.id);
            // 输了的玩家弃牌
            GiveUpCard(loseDto.id);
            resultDto.Change(winDto,loseDto);
            Console.WriteLine("胜利者：" + winDto.name + "  失败者: " + loseDto.name);
            room.Broadcast(OpCode.Fight, FightCode.CompareCard_BRO,resultDto, otherClient);
        }

        /// <summary>
        /// 比牌的逻辑算法
        /// </summary>
        private void CompareCard(FightRoom room,ClientPeer c1Client, PlayerDto c1, PlayerDto c2,ClientPeer otherClient) {
            bool c1Win = false;
            // 牌型比较
            if (c1.cardType > c2.cardType) { // c1 胜
                c1Win = true;
            }
            else if (c1.cardType == c2.cardType) { // 牌型相同
                                                   // 单张
                if (c1.cardType == CardType.Sin) {
                    c1Win = CompareSinCard(room,c1Client,c1, c2,otherClient);
                }

                // 对子 662 663 / 766 866 / 662 966
                if (c1.cardType == CardType.Double) {
                    int c1Double = 0, c1Sin = 0, c2Double = 0, c2Sin = 0;
                    // c1
                    if (c1.cardLidt[0].Weight == c1.cardLidt[1].Weight) { // 对子在前
                        c1Double = c1.cardLidt[0].Weight;
                        c1Sin = c1.cardLidt[2].Weight;
                    }
                    if (c1.cardLidt[1].Weight == c1.cardLidt[2].Weight) { // 对子在后
                        c1Double = c1.cardLidt[1].Weight;
                        c1Sin = c1.cardLidt[0].Weight;
                    }
                    // c2
                    if (c2.cardLidt[0].Weight == c2.cardLidt[1].Weight) { // 对子在前
                        c2Double = c2.cardLidt[0].Weight;
                        c2Sin = c2.cardLidt[2].Weight;
                    }
                    if (c2.cardLidt[1].Weight == c2.cardLidt[2].Weight) { // 对子在后
                        c2Double = c2.cardLidt[1].Weight;
                        c2Sin = c2.cardLidt[0].Weight;
                    }
                    // 比较对子
                    if (c1Double > c2Double) {
                        c1Win = true;
                    }
                    else if (c1Double == c2Double) {
                        // 比较单张
                        if (c1Sin > c2Sin) {
                            c1Win = true;
                        }
                        else {
                            c1Win = false;
                        }
                    }
                    else {
                        c1Win = false;
                    }
                }

                // 顺子,顺金，豹子，都直接比较 3 张牌加起来的值，谁大谁赢
                if (c1.cardType == CardType.Sequence || c1.cardType == CardType.SGolden || c1.cardType == CardType.Leopard) {
                    // 获取和
                    int c1Sum = 0, c2Sum = 0;
                    for (int i = 0; i < c1.cardLidt.Count; i++) {
                        c1Sum += c1.cardLidt[i].Weight;
                    }
                    for (int i = 0; i < c2.cardLidt.Count; i++) {
                        c2Sum += c2.cardLidt[i].Weight;
                    }
                    // 比较和
                    if (c1Sum > c2Sum) {
                        c1Win = true;
                    }
                    else {
                        c1Win = false;
                    }
                }

                // 金花
                if (c1.cardType == CardType.SGolden) {
                    c1Win = CompareSinCard(room,c1Client,c1, c2,otherClient);
                }

                // Max 235
                if (c1.cardType == CardType.Max) {
                    c1Win = false;
                }

            }
            else { // c1 输
                c1Win = false;
            }

            if (c1Win) {
                CompareResult(room,c1Client,c1,c2,otherClient);
            }
            else {
                CompareResult(room,c1Client,c2,c1,otherClient);
            }
        }

        /// <summary>
        /// 按单张比较牌
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <returns></returns>
        private bool CompareSinCard(FightRoom room, ClientPeer c1Client, PlayerDto c1, PlayerDto c2, ClientPeer otherClient) {
            bool c1Win = false;
            // 第一张
            if (c1.cardLidt[0].Weight > c2.cardLidt[0].Weight) {
                c1Win = true;
            }
            else if (c1.cardLidt[0].Weight == c2.cardLidt[0].Weight) {
                // 第 2 张
                if (c1.cardLidt[1].Weight > c2.cardLidt[1].Weight) {
                    c1Win = true;
                }
                else if (c1.cardLidt[1].Weight == c2.cardLidt[1].Weight) {
                    // 第 3 张
                    if (c1.cardLidt[2].Weight > c2.cardLidt[2].Weight) {
                        c1Win = true;
                    }
                    else {
                        c1Win = false;
                    }
                }
                else {
                    c1Win = false;
                }
            }
            else {
                c1Win = false;
            }
            return c1Win;
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

        /// <summary>
        /// 弃牌，不需要转换下注
        /// </summary>
        /// <param name="userId"></param>
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
                // 胜利的玩家
                if (!room.IsGiveUpCard(player.id) && !room.IsLeaveRoom(player.id)) {
                    winPlayer = player;
                }
                // 失败的玩家们
                else {
                    loseList.Add(player);
                }
            }
            DatabaseManager.UpdateCoin(winPlayer.id, room.stakesSum);
            gameOverDto.Change(winPlayer, loseList, room.stakesSum);
            room.Broadcast(OpCode.Fight, FightCode.GameOver_BRO, gameOverDto);
            //销毁房间
            fightCache.DesRoom(room);
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
        private void Turn(ClientPeer client, int turnUserId = -1) {
            // 清空计时器任务
            TimerManager.Instance.Clear();
            if (fightCache.IsFighting(client.Id) == false) {
                return;
            }
            FightRoom room = fightCache.GetFightRoomByUserId(client.Id);
            int nextID = room.Turn();
            if (room.IsGiveUpCard(nextID) || room.IsLeaveRoom(nextID) || (turnUserId != -1 && nextID != turnUserId)) {
                // 如果下一位玩家离开或弃牌了，就继续轮换下注，直到改玩家不离开也不弃牌为止
                Turn(client,turnUserId);
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
                    GameOver(room);
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
