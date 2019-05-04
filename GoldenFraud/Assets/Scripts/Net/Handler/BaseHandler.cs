using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 模块的基类
/// </summary>
public abstract class BaseHandler {

    /// <summary>
    /// 接收到数据
    /// </summary>
	public abstract void OnReceive(int subCode, object value);
}
