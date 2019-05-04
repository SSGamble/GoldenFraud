using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Protocol.Dto {
    /// <summary>
    /// 用户信息传输模型
    /// </summary>
    [Serializable] // 序列化
    public class UserDto {
        public int id;
        public string name;
        public string iconName;
        public int coin;

        public UserDto(int id,string name,string iconName,int coin) {
            this.id = id;
            this.name = name;
            this.iconName = iconName;
            this.coin = coin;
        }

        public void Change(int id, string name, string iconName, int coin) {
            this.id = id;
            this.name = name;
            this.iconName = iconName;
            this.coin = coin;
        }
    }
}
