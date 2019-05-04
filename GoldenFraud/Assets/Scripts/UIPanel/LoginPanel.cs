using System.Collections;
using System.Collections.Generic;
using Protocol.Code;
using Protocol.Dto;
using UnityEngine;
using UnityEngine.UI;

public class LoginPanel : BasePanel {

    private InputField iptUserName;
    private InputField iptPwd;
    private Button btnLogin;
    private Button btnRegister;

    private void Awake() {
        EventCenter.AddListener(EventType.ShowLoginPanel, ShowLoginPanel);
        Init();
    }

    private void Init() {
        iptUserName = transform.Find("iptUserName").GetComponent<InputField>();
        iptPwd = transform.Find("iptPwd").GetComponent<InputField>();
        btnLogin = transform.Find("btnLogin").GetComponent<Button>();
        btnLogin.onClick.AddListener(OnLoginButtonClick);
        btnRegister = transform.Find("btnRegister").GetComponent<Button>();
        btnRegister.onClick.AddListener(OnRegisterButtonClick);
    }

    private void OnDestroy() {
        EventCenter.RemoveListener(EventType.ShowLoginPanel, ShowLoginPanel);
    }

    /// <summary>
    /// 显示登录面板
    /// </summary>
    private void ShowLoginPanel() {

    }

    #region 按钮的点击事件

    /// <summary>
    /// 注册按钮点击事件
    /// </summary>
    private void OnRegisterButtonClick() {
        UIManager.Instance.PushPanel(UIPanelType.Register); // 将面板入栈
        EventCenter.Broadcast(EventType.ShowRegisterPanel);
    }

    /// <summary>
    /// 登录按钮点击事件
    /// </summary>
    private void OnLoginButtonClick() {
        if (iptUserName.text == null || iptUserName.text == "" || iptPwd.text == null || iptPwd.text == "") {
            EventCenter.Broadcast(EventType.Hint, "用户名和密码不能为空");
            return;
        }
        // 向服务器发送登录请求
        AccountDto dto = new AccountDto(iptUserName.text, iptPwd.text);
        NetMsgCenter.Instance.SendMsg(OpCode.Account, AccountCode.Login_CREQ, dto);
    }

    #endregion

    #region 面板的生命周期函数

    public override void OnEnter() {
        base.OnEnter();
    }

    public override void OnExit() {
        base.OnExit();
    }

    public override void OnPause() {
        base.OnPause();
    }

    public override void OnResume() {
        base.OnResume();
    }

    #endregion
}
