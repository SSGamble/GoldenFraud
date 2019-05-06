using Protocol.Fight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Cache.Fight
{
    /// <summary>
    /// 牌库
    /// </summary>
    public class CardLibrary
    {
        private Queue<CardDto> cardQueue = new Queue<CardDto>();

        public CardLibrary()
        {
            InitCard();
            Shuffle();
        }

        public void Init()
        {
            InitCard(); // 初始化牌
            Shuffle(); // 洗牌
        }

        /// <summary>
        /// 初始化牌
        /// </summary>
        private void InitCard()
        {
            cardQueue.Clear(); // 先清空
            for (int weight = 2; weight <= 14; weight++)    // 2 - A
            { 
                for (int color = 0; color <= 3; color++)
                {
                    string name = "card_" + color + "_" + weight;
                    CardDto card = new CardDto(name,weight, color);
                    cardQueue.Enqueue(card);
                }
            }
        }

        /// <summary>
        /// 洗牌
        /// </summary>
        private void Shuffle()
        {
            List<CardDto> cardList =  cardQueue.ToList<CardDto>();
            Random random = new Random();
            // 遍历牌库，将该张牌和牌库里随机一张牌交换
            for (int i = 0; i < cardQueue.Count; i++)
            {
                int ran = random.Next(0, cardList.Count);
                CardDto temp = cardList[i];
                cardList[i] = cardList[ran];
                cardList[ran] = temp;
            }

            cardQueue.Clear();
            foreach (var card in cardList)
            {
                cardQueue.Enqueue(card);
            }
        }

        /// <summary>
        /// 出牌
        /// </summary>
        /// <returns></returns>
        public CardDto DealCard()
        {
            if (cardQueue.Count<9)
            {
                InitCard();
                Shuffle();
            }
            return cardQueue.Dequeue();
        }
    }
}
