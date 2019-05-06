using Protocol.Constant;
using Protocol.Fight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Protocol.Dto
{
    /// <summary>
    /// 玩家传输模型
    /// </summary>
    [Serializable]
    public class PlayerDto
    {
        public int id { get; set; }
        public string name { get; set; }
        /// <summary>
        /// 下注总数
        /// </summary>
        public int stakesSum { get; set; }
        /// <summary>
        /// 身份
        /// </summary>
        public Identity identity { get; set; }
        /// <summary>
        /// 自己的手牌
        /// </summary>
        public List<CardDto> cardLidt;
        /// <summary>
        /// 手牌类型
        /// </summary>
        public CardType cardType;

        public PlayerDto(int id,string name)
        {
            this.id = id;
            this.name = name;
            stakesSum = 0;
            identity = Identity.Normal;
            cardLidt = new List<CardDto>();
            cardType = CardType.None;
        }

        /// <summary>
        /// 添加卡牌
        /// </summary>
        /// <param name="card"></param>
        public void AddCard(CardDto card)
        {
            cardLidt.Add(card);
        }

        /// <summary>
        /// 移除卡牌
        /// </summary>
        /// <param name="card"></param>
        public void RemoveCard(CardDto card)
        {
            cardLidt.Remove(card);
        }
    }
}
