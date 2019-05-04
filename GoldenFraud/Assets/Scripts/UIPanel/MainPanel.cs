using System;
using System.Collections;
using System.Collections.Generic;
using Protocol.Code;
using UnityEngine;
using UnityEngine.UI;

public class MainPanel : BasePanel {

    private Text txtName;
    private Text txtCoin;
    private Image imgHead;
    private Button btnRank;
    private Button btnBank;
    private Button btnStand;
    private Button btnOnline;

    private void Awake() {
        EventCenter.AddListener<int>(EventType.UpdateCoin, UpdateCoin);
        Init();
    }

    private void OnDestroy() {
        EventCenter.RemoveListener<int>(EventType.UpdateCoin, UpdateCoin);
    }

    private void Init() {
        txtName = transform.Find("name/txtName").GetComponent<Text>();
        txtCoin = transform.Find("coin/txtCoin").GetComponent<Text>();
        imgHead = transform.Find("Mask/imgHead").GetComponent<Image>();
        btnRank = transform.Find("btnRank").GetComponent<Button>();
        btnRank.onClick.AddListener(OnRankButtonClick);
        btnBank = transform.Find("btnBank").GetComponent<Button>();
        btnBank.onClick.AddListener(OnBankButtonClick);
        btnStand = transform.Find("btnStand").GetComponent<Button>();
        btnStand.onClick.AddListener(OnStandButtonClick);
        btnOnline = transform.Find("btnOnline").GetComponent<Button>();
        btnOnline.onClick.AddListener(OnOnlineButtonClick);

        // 显示用户信息
        txtName.text = Models.GameModel.userDto.name;
        txtCoin.text = Models.GameModel.userDto.coin.ToString();
        imgHead.sprite = ResourcesManager.GetSprite(Models.GameModel.userDto.iconName);
    }

    /// <summary>
    /// 更新金币
    /// </summary>
    /// <param name="value"></param>
    private void UpdateCoin(int value) {
        txtCoin.text = value.ToString();
    }

    #region 按钮点击事件

    private void OnBankButtonClick() {
        EventCenter.Broadcast(EventType.ShowBankPanel);
    }

    private void OnRankButtonClick() {
        // 向服务器发送获取排行榜的请求
        NetMsgCenter.Instance.SendMsg(OpCode.Account, AccountCode.GetRank_CREQ, null);
        EventCenter.Broadcast(EventType.ShowRankPanel);
    }

    private void OnOnlineButtonClick() {
        EventCenter.Broadcast(EventType.ShowRoomChoosePanel,GameType.Online);
    }

    private void OnStandButtonClick() {
        EventCenter.Broadcast(EventType.ShowRoomChoosePanel,GameType.Stand);
    }

    #endregion
}
