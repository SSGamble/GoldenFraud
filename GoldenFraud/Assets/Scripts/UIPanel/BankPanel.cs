using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Protocol.Code;

public class BankPanel : MonoBehaviour {

    private GameObject goods;
    private Button[] goodsBtnArr; // 通过遍历 goods 下的子物体来获得
    private Button btnClose;
    private int rechargeCount; // 充值金币数量

    private void Awake() {
        EventCenter.AddListener(EventType.ShowBankPanel, ShowBankPanel);
        EventCenter.AddListener<int>(EventType.UpdateCoin, UpdateCoin);
        Init();
    }

    private void OnDestroy() {
        EventCenter.RemoveListener(EventType.ShowBankPanel, ShowBankPanel);
        EventCenter.RemoveListener<int>(EventType.UpdateCoin, UpdateCoin);
    }

    private void Init() {
        btnClose = transform.Find("btnClose").GetComponent<Button>();
        btnClose.onClick.AddListener(OnCloseButtonClick);

        goods = transform.Find("goods").gameObject;
        goodsBtnArr = new Button[goods.transform.childCount];
        // 遍历 goods 下的子物体 Button
        for (int i = 0; i < goods.transform.childCount; i++) {
            goodsBtnArr[i] = goods.transform.GetChild(i).GetComponentInChildren<Button>();
        }

        goodsBtnArr[0].onClick.AddListener(delegate { Recharge(10); });
        goodsBtnArr[1].onClick.AddListener(delegate { Recharge(20); });
        goodsBtnArr[2].onClick.AddListener(delegate { Recharge(50); });
        goodsBtnArr[3].onClick.AddListener(delegate { Recharge(100); });
        goodsBtnArr[4].onClick.AddListener(delegate { Recharge(200); });
        goodsBtnArr[5].onClick.AddListener(delegate { Recharge(500); });
    }

    /// <summary>
    /// 充值
    /// </summary>
    /// <param name="coin"></param>
    private void Recharge(int coin) {
        rechargeCount = coin;
        NetMsgCenter.Instance.SendMsg(OpCode.Account, AccountCode.UpdateCoin_CREQ, coin);
    }

    /// <summary>
    /// 更新金币数量
    /// </summary>
    /// <param name="value"></param>
    private void UpdateCoin(int value) {
        EventCenter.Broadcast(EventType.Hint, "充值 " + rechargeCount + " 金币成功");
    }

    private void OnCloseButtonClick() {
        transform.DOScale(Vector3.zero, 0.3f);
    }

    private void ShowBankPanel() {
        transform.DOScale(Vector3.one, 0.3f);
    }
}
