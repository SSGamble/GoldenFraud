using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Cache {

    /// <summary>
    /// 数据缓存单一模式，防止数据缓存存在多份，打破了数据的唯一性，整个服务器只会存在一个房间的数据缓存
    /// </summary>
    public class Caches {

        // 因为有 3 种类型的房间，所有给 3 个数据缓存
        public static List<MatchCache> matchCacheList { get; set; }

        static Caches() {
            matchCacheList = new List<MatchCache>();
            for (int i = 0; i < 3; i++) {
                matchCacheList.Add(new MatchCache());
            }
        }
    }
}
