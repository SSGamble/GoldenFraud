using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Protocol.Dto {

    /// <summary>
    /// 排行榜单个用户信息传输模型
    /// </summary>
    [Serializable]
    public class RankItemDto {

        public string name;
        public int coin;

        public RankItemDto(string name, int coin) {
            this.name = name;
            this.coin = coin;
        }

        public void Change(string name, int coin) {
            this.name = name;
            this.coin = coin;
        }
    }
}
