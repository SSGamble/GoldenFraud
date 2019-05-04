using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Protocol.Code {
    /// <summary>
    /// 账户模块下的子操作码
    /// </summary>
    public class AccountCode {

        // 注册 
        public const int Register_CREQ = 0; // 客户端请求
        public const int Register_SRES = 1; // 服务器响应

        // 登录 
        public const int Login_CREQ = 2; // 客户端请求
        public const int Login_SRES = 3; // 服务器响应

        // 获取用户信息
        public const int GetUserInfo_CREQ = 4;
        public const int GetUserInfo_SRES = 5;

        // 获取排行榜信息
        public const int GetRank_CREQ = 6;
        public const int GetRank_SRES = 7;

        // 充值
        public const int UpdateCoin_CREQ = 8;
        public const int UpdateCoin_SRES = 9;
    }
}
