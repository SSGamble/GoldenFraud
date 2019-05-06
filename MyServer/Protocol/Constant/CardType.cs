using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Protocol.Constant
{
    /// <summary>
    /// 手牌类型
    /// </summary>
    public enum CardType
    {
        None,
        Sin, // 单张
        Double, // 对子
        Sequence, // 顺子
        Golden, // 金花
        SGolden,// 顺金
        Leopard, // 豹子
        Max // 235
    }
}
