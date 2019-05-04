using System.Collections;
using System.Collections.Generic;
using Protocol.Code;
using Protocol.Dto;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RegisterPanel : BasePanel {

    private InputField iptUserName;
    private InputField iptPwd;
    private Button btnIsShowPwd;
    private Button btnRegister;
    private Button btnBack;

    private bool isShowPwd = false; // 是否显示密码

    private void Awake() {
        EventCenter.AddListener(EventType.ShowRegisterPanel, ShowRegisterPanel);
        Init();
    }

    private void Init() {
        iptUserName = transform.Find("UserName/iptUserName").GetComponent<InputField>();
        iptPwd = transform.Find("Password/iptPwd").GetComponent<InputField>();
        btnBack = transform.Find("btnBack").GetComponent<Button>();
        btnBack.onClick.AddListener(OnBackButtonClick);
        btnIsShowPwd = transform.Find("btnIsShowPwd").GetComponent<Button>();
        btnIsShowPwd.onClick.AddListener(OnIsShowPwdButtonClick);
        btnRegister = transform.Find("btnRegister").GetComponent<Button>();
        btnRegister.onClick.AddListener(OnRegisterButtonClick);
    }

    private void OnDestroy() {
        EventCenter.RemoveListener(EventType.ShowRegisterPanel, ShowRegisterPanel);
    }

    /// <summary>
    /// 显示注册面板
    /// </summary>
    private void ShowRegisterPanel() {

    }

    #region 按钮的点击事件

    /// <summary>
    /// 注册按钮点击事件
    /// </summary>
    private void OnRegisterButtonClick() {
        if (iptUserName.text == null || iptUserName.text == "" || iptPwd.text == null || iptPwd.text == "") {
            //Debug.Log("用户名和密码不能为空");
            EventCenter.Broadcast(EventType.Hint, "用户名和密码不能为空");
            return;
        }
        // 向服务器发送数据，注册用户
        AccountDto dto = new AccountDto(iptUserName.text, iptPwd.text); // 账号传输模型
        NetMsgCenter.Instance.SendMsg(OpCode.Account, AccountCode.Register_CREQ, dto);
    }
    
    /// <summary>
    /// 是否显示密码按钮点击事件
    /// </summary>
    private void OnIsShowPwdButtonClick() {
        isShowPwd = !isShowPwd;

        if (isShowPwd) { // 显示密码
            iptPwd.contentType = InputField.ContentType.Standard;
            btnIsShowPwd.GetComponentInChildren<Text>().text = "隐藏"; // 将按钮上的字设为隐藏
        }
        else { // 隐藏密码
            iptPwd.contentType = InputField.ContentType.Password;
            btnIsShowPwd.GetComponentInChildren<Text>().text = "显示"; 
        }
        // 设置聚焦密码框，否则需要聚焦后才会改变
        EventSystem.current.SetSelectedGameObject(iptPwd.gameObject);
    }

    /// <summary>
    /// 返回按钮点击事件
    /// </summary>
    private void OnBackButtonClick() {
        UIManager.Instance.PopPanel(); // 出栈
        EventCenter.Broadcast(EventType.ShowLoginPanel);
    }

    #endregion

    #region 面板的生命周期函数

    public override void OnEnter() {
        base.OnEnter();
    }

    public override void OnExit() {
        Destroy(gameObject); // 销毁面板
    }

    public override void OnPause() {
        base.OnPause();
    }

    public override void OnResume() {
        base.OnResume();
    }

    #endregion
}
