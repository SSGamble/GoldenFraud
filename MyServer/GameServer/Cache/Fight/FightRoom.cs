using GameServer.Database;
using MyServer;
using Protocol.Constant;
using Protocol.Dto;
using Protocol.Fight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Cache.Fight
{
    /// <summary>
    /// 战斗房间
    /// </summary>
    public class FightRoom
    {
        /// <summary>
        /// 房间ID，唯一标识
        /// </summary>
        public int roomId;
        /// <summary>
        /// 房间内的玩家
        /// </summary>
        public List<PlayerDto> playerList;
        /// <summary>
        /// 牌库
        /// </summary>
        public CardLibrary cardLibrary;
        /// <summary>
        /// 回合管理类
        /// </summary>
        public RoundModel roundModel;
        /// <summary>
        /// 离开玩家id 列表
        /// </summary>
        public List<int> leaveUserIdList = new List<int>();
        /// <summary>
        /// 弃牌玩家id列表
        /// </summary>
        public List<int> giveUpUserIdList = new List<int>();
        /// <summary>
        /// 顶注
        /// </summary>
        public int topStakes;
        /// <summary>
        /// 底注
        /// </summary>
        public int bottomStakes;
        /// <summary>
        /// 上一位玩家下注数量
        /// </summary>
        public int lastPlayerStakes;
        /// <summary>
        /// 总下注数
        /// </summary>
        public int stakesSum;
        /// <summary>
        /// 庄家在玩家列表中的下标
        /// </summary>
        public int bankerIndex = -1;

        public FightRoom(int roomId,List<ClientPeer> clientList)
        {
            this.roomId = roomId;
            playerList = new List<PlayerDto>();
            foreach (var client in clientList)
            {
                PlayerDto playerDto = new PlayerDto(client.Id, client.UserName);
                playerList.Add(playerDto);
            }
            cardLibrary = new CardLibrary();
            roundModel = new RoundModel();
            leaveUserIdList = new List<int>();
            giveUpUserIdList = new List<int>();
            stakesSum = 0;
        }

        public void Init(List<ClientPeer> clientList)
        {
            stakesSum = 0;
            playerList.Clear();
            foreach (var client in clientList)
            {
                PlayerDto playerDto = new PlayerDto(client.Id, client.UserName);
                playerList.Add(playerDto);
            }
        }

        /// <summary>
        /// 随机一个庄家
        /// </summary>
        public ClientPeer SetBanker()
        {
            Random ran = new Random();
            int index = ran.Next(0, playerList.Count);
            bankerIndex = index;
            int userId = playerList[index].id;
            playerList[index].identity = Identity.Banker;
            // 默认庄家第一个下注，实际应该是他的下一位玩家下注，之后游戏开始后会轮换到下一位
            roundModel.Start(userId);
            ClientPeer bankerCllient = DatabaseManager.GetClientPeerByUserId(userId);
            Console.WriteLine(bankerCllient.UserName);
            return bankerCllient;
        }

        /// <summary>
        /// 发牌
        /// </summary>
        public void DealCard()
        {
            // 9 张牌
            for (int i = 0; i < 9; i++)
            {
                // 依次给 3 个玩家发牌
                playerList[bankerIndex].AddCard(cardLibrary.DealCard());
                bankerIndex++;
                if (bankerIndex>playerList.Count-1)
                {
                    bankerIndex = 0;
                }
            }
        }

        /// <summary>
        /// 对牌排序
        /// </summary>
        /// <param name="cardList"></param>
        private void SortCard(ref List<CardDto> cardList)
        {
            for (int i = 0; i < cardList.Count-1; i++)
            {
                for (int j = 0; j < cardList.Count -1-i; j++)
                {
                    if (cardList[j].Weight<cardList[j+1].Weight)
                    {
                        CardDto temp = cardList[j];
                        cardList[j] = cardList[j + 1];
                        cardList[j + 1] = temp;
                    }
                }
            }
        }

        /// <summary>
        /// 对房间内所有玩家的手牌进行排序
        /// </summary>
        public void SortAllPlayerCard()
        {
            foreach (var player in playerList)
            {
                SortCard(ref player.cardLidt);
            }
        }

        /// <summary>
        /// 获取所有玩家牌型
        /// </summary>
        public void GetAllPlayerCardType()
        {
            foreach (var player in playerList)
            {
                player.cardType = GetCardType(player.cardLidt);
            }
        }

        /// <summary>
        /// 获取牌型
        /// </summary>
        protected CardType GetCardType(List<CardDto> cardList)
        {
            CardType cardType = CardType.None;
            // 235，因为前面从大到小排了序，所以反过来就是 532
            if (cardList[0].Weight == 5 && cardList[1].Weight == 3 && cardList[2].Weight == 2)
            {
                cardType = CardType.Max;
            }
            // 豹子，3 张一样，666
            else if (cardList[0].Weight == cardList[1].Weight && cardList[1].Weight == cardList[2].Weight)
            {
                cardType = CardType.Leopard;
            }
            // 顺金，是顺子，并且花色一样，765
            else if (cardList[0].Color == cardList[1].Color && cardList[1].Color == cardList[2].Color &&
                cardList[0].Weight == cardList[1].Weight + 1 && cardList[1].Weight == cardList[2].Weight + 1)
            {
                cardType = CardType.SGolden;
            }
            // 金花，颜色一样
            else if (cardList[0].Color == cardList[1].Color && cardList[1].Color == cardList[2].Color)
            {
                cardType = CardType.Golden;
            }
            // 顺子，765
            else if (cardList[0].Weight == cardList[1].Weight + 1 && cardList[1].Weight == cardList[2].Weight + 1)
            {
                cardType = CardType.Sequence;
            }
            // 对子，668,688（已排过序）
            else if (cardList[0].Weight == cardList[1].Weight || cardList[1].Weight == cardList[2].Weight)
            {
                cardType = CardType.Double;
            }
            // 单张
            else
            {
                cardType = CardType.Sin;
            }
            return cardType;
        }

        /// <summary>
        /// 是否离开房间
        /// </summary>
        public bool IsLeaveRoom(int id)
        {
            return leaveUserIdList.Contains(id);
        }

        /// <summary>
        /// 是否弃牌
        /// </summary>
        public bool IsGiveUpCard(int id)
        {
            return giveUpUserIdList.Contains(id);
        }

        /// <summary>
        /// 广播发消息，给除自己以外的客户端发消息
        /// </summary>
        public void Broadcast(int opCode, int subCode, object value, ClientPeer exceptClient = null)
        {
            NetMsg msg = new NetMsg(opCode, subCode, value);
            byte[] data = EncodeTool.EncodeMsg(msg);
            byte[] packet = EncodeTool.EncodePacket(data);
            foreach (var player in playerList)
            {
                ClientPeer client = DatabaseManager.GetClientPeerByUserId(player.id);
                if (client == exceptClient)
                {
                    continue;
                }
                client.SendMsg(packet);
            }
        }
        
        /// <summary>
        /// 轮换下注
        /// </summary>
        /// <returns>下一次下注的玩家 id</returns>
        public int Turn()
        {
            int currentUserId = roundModel.CurrentStakesUserId;
            int nextUserId = GetNextStakesUserId(currentUserId);
            roundModel.Turn(nextUserId);
            return nextUserId;
        }

        /// <summary>
        /// 获取下一次下注的用户id
        /// </summary>
        /// <param name="currentId"></param>
        /// <returns></returns>
        private int GetNextStakesUserId(int currentId)
        {
            for (int i = 0; i < playerList.Count; i++)
            {
                if (playerList[i].id == currentId)
                {
                    if (i == playerList.Count - 1)
                    {
                        return playerList[0].id;
                    }
                    else
                    {
                        return playerList[i+1].id;
                    }
                }
            }
            return -1;
        }

        /// <summary>
        /// 更新玩家的下注总数
        /// </summary>
        /// <param name="id"></param>
        /// <param name="stakesSum"></param>
        public int UpdatePlayerStakesSum(int id,int stakes)
        {
            foreach (var player in playerList)
            {
                if (player.id == id)
                {
                    player.stakesSum += stakes;
                    return player.stakesSum;
                }
            }
            return 0;
        }

        /// <summary>
        /// 销毁房间数据
        /// </summary>
        public void Destory()
        {
            playerList.Clear();
            cardLibrary.Init();
            roundModel.Init();
            leaveUserIdList.Clear();
            giveUpUserIdList.Clear();
            stakesSum = 0;
            bankerIndex = -1;
        }

        /// <summary>
        /// 重置位置，给 3 个玩家排序
        /// </summary>
        public void ResetPosition(int bankerId) {
            // x a b
            if (playerList[0].id == bankerId) {
                PlayerDto dto = playerList[1];
                playerList[1] = playerList[2];
                playerList[2] = dto;
            }
            // a x b
            if (playerList[1].id == bankerId) {
                PlayerDto dto = playerList[0];
                playerList[0] = playerList[2];
                playerList[2] = dto;
            }
            // a b x
            if (playerList[2].id == bankerId) {
                PlayerDto dto = playerList[0];
                playerList[0] = playerList[1];
                playerList[1] = dto;
            }
        }
    }
}
