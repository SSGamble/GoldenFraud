using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Protocol.Fight
{
    /// <summary>
    /// 卡牌传输模型
    /// </summary>
    [Serializable]
    public class CardDto
    {
        public string Name { set; get; }
        public int Weight { set; get; }
        public int Color { set; get; }

        public CardDto(string name,int weight,int color)
        {
            this.Name = name;
            this.Weight = weight;
            this.Color = color;
        }
    }
}
