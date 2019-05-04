using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 面板的基类，所有的面板继承自他
///     管理面板的 4 种状态
/// </summary>
public class BasePanel : MonoBehaviour {

    /// <summary>
    /// 界面进入
    /// </summary>
    public virtual void OnEnter()
    {

    }

    /// <summary>
    /// 界面暂停
    /// </summary>
    public virtual void OnPause()
    {

    }

    /// <summary>
    /// 界面继续
    /// </summary>
    public virtual void OnResume()
    {

    }

    /// <summary>
    /// 界面退出
    /// </summary>
    public virtual void OnExit()
    {

    }
}
