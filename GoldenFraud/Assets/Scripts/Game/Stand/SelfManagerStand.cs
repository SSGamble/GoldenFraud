using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 单机 - 玩家管理
/// </summary>
public class SelfManagerStand : MonoBehaviour {

    private GameObject goBottomButton;
    private GameObject goCountDown;
    private GameObject txtReady;
    private Image imgHead;
    private Image imgBanker;
    private Text txtName;
    private Text txtCoin;
    private Text txtStakesSum;
    private Text txtCountDown;
    private Button btnReady;
    private Transform cardPoint;

    private void Awake() {
        Init();
    }

    private void Init() {
        goBottomButton = transform.Find("goBottomButton").gameObject;
        imgHead = transform.Find("imgHead").GetComponent<Image>();
        imgBanker = transform.Find("imgBanker").GetComponent<Image>();
        txtName = transform.Find("txtName").GetComponent<Text>();
        txtCoin = transform.Find("Coin/txtCoin").GetComponent<Text>();
        txtReady = transform.Find("txtReady").gameObject;
        txtCountDown = transform.Find("CountDown/txtCountDown").GetComponent<Text>();
        txtCoin = transform.Find("Coin/txtCoin").GetComponent<Text>();
        btnReady = transform.Find("btnReady").GetComponent<Button>();
        cardPoint = transform.Find("cardPoint");

        goBottomButton.SetActive(false);
        imgBanker.gameObject.SetActive(false);
        txtReady.SetActive(false);
        goCountDown.SetActive(false);
    }
}
