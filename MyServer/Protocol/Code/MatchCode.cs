using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Protocol.Code {
    /// <summary>
    /// 匹配的子操作码
    /// </summary>
    public class MatchCode {

        // 进入房间
        public const int Enter_CREQ = 0; // 客户端请求
        public const int Enter_SRES = 1; // 服务器响应
        public const int Enter_BRO = 2; // 客户端广播，进入房间后需要广播给其他玩家

        // 离开房间
        public const int Leave_CREQ = 3; 
        public const int Leave_BRO = 4;

        // 准备
        public const int Ready_CREQ = 5;
        public const int Ready_BRO = 6;

        // 取消准备
        public const int UnReady_CREQ = 7;
        public const int UnReady_BRO = 8;

        // 开始游戏
        public const int StartGame_BRO = 9;
    }
}
