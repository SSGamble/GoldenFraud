using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

/// <summary>
/// 网络消息类
/// 作用：每次发送消息都发送这个类，接收到消息后也需要转换成这个类
/// </summary>
public class NetMsg {

    /// <summary>
    /// 操作码
    /// </summary>
    public int opCode { get; set; }
    /// <summary>
    /// 子操作码
    /// </summary>
    public int subCode { get; set; }
    /// <summary>
    /// 传递的参数
    /// </summary>
    public object value { get; set; }

    public NetMsg() {

    }

    public NetMsg(int opCode, int subCode, object value) {
        this.opCode = opCode;
        this.subCode = subCode;
        this.value = value;
    }

    /// <summary>
    /// 防止每次 new NetMsg 这个类，现在是要调用 Change方法，改变这几个值就可以了
    /// </summary>
    /// <param name="opCode"></param>
    /// <param name="subCode"></param>
    /// <param name="value"></param>
    public void Change(int opCode, int subCode, object value) {
        this.opCode = opCode;
        this.subCode = subCode;
        this.value = value;
    }
}
