using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Protocol.Code;
using UnityEngine;
using UnityEngine.UI;

public class SelfManagerOnlint : MonoBehaviour {

    public GameObject goCardPre;
    protected GameObject goCountDown;
    protected Image imgHead;
    protected Image imgBanker;
    protected Text txtStakesSum;
    protected Text txtCountDown;
    protected Transform cardPoint;
    private GameObject goBottomButton;
    private Text txtHint;
    private GameObject goCompareBtns;
    private Text txtName;
    private Text txtCoin;
    private Button btnReady;
    private Button btnUnReady;
    private Button btnLookCard;
    private Button btnFollowStakes;
    private Button btnGiveUp;
    private Button btnCompareCard;
    private Button btnAddStakes;
    private Button btnCompareLeft;
    private Button btnCompareRight;
    private Toggle tog2;
    private Toggle tog5;
    private Toggle tog10;

    protected StakesHint stakesHint;
    protected List<GameObject> goCardList = new List<GameObject>(); // 自身的 3 张牌
    protected float cardPointX = -40f; // 牌的到达位置
    protected bool isStartStakes = false; // 开始下注的标志位，用于计时
    protected float time = 60f; // 倒计时
    protected float timer = 0.0f; // 计时器

    private void Awake() {
        EventCenter.AddListener(EventType.StartGame, StartGame);
        Init();
    }


    private void OnDestroy()
    {
        EventCenter.RemoveListener(EventType.StartGame, StartGame);
    }

    private void Init() {
        goCompareBtns = transform.Find("goCompareBtns").gameObject;
        btnCompareLeft = goCompareBtns.transform.Find("btnCompareLeft").GetComponent<Button>();
        btnCompareLeft.onClick.AddListener(OnCompareLeftButtonClick);
        btnCompareRight = goCompareBtns.transform.Find("btnCompareRight").GetComponent<Button>();
        btnCompareRight.onClick.AddListener(OnCompareRightButtonClick);
        goBottomButton = transform.Find("goBottomButton").gameObject;
        goCountDown = transform.Find("goCountDown").gameObject;
        txtHint = transform.Find("txtHint").GetComponent<Text>();
        imgHead = transform.Find("imgHead").GetComponent<Image>();
        imgBanker = transform.Find("imgBanker").GetComponent<Image>();
        txtName = transform.Find("txtName").GetComponent<Text>();
        txtCoin = transform.Find("Coin/txtCoin").GetComponent<Text>();
        txtCountDown = transform.Find("goCountDown/txtCountDown").GetComponent<Text>();
        txtStakesSum = transform.Find("StakesSum/txtStakesSum").GetComponent<Text>();
        txtCoin = transform.Find("Coin/txtCoin").GetComponent<Text>();
        stakesHint = transform.Find("txtStakesHint").GetComponent<StakesHint>();
        cardPoint = transform.Find("cardPoint");
        tog2 = transform.Find("goBottomButton/tog2").GetComponent<Toggle>();
        tog5 = transform.Find("goBottomButton/tog5").GetComponent<Toggle>();
        tog10 = transform.Find("goBottomButton/tog10").GetComponent<Toggle>();
        btnReady = transform.Find("btnReady").GetComponent<Button>();
        btnReady.onClick.AddListener(OnReadyButtonClick);
        btnUnReady = transform.Find("btnUnReady").GetComponent<Button>();
        btnUnReady.onClick.AddListener(OnUnReadyButtonClick);
        btnLookCard = transform.Find("goBottomButton/btnLookCard").GetComponent<Button>();
        btnLookCard.onClick.AddListener(OnLookCardBtnClick);
        btnFollowStakes = transform.Find("goBottomButton/btnFollowStakes").GetComponent<Button>();
        btnFollowStakes.onClick.AddListener(OnFollowBtnClick);
        btnGiveUp = transform.Find("goBottomButton/btnGiveUp").GetComponent<Button>();
        btnGiveUp.onClick.AddListener(OnGiveUpButtonClick);
        btnCompareCard = transform.Find("goBottomButton/btnCompareCard").GetComponent<Button>();
        btnCompareCard.onClick.AddListener(OnCompareBtnClick);
        btnAddStakes = transform.Find("goBottomButton/btnAddStakes").GetComponent<Button>();
        btnAddStakes.onClick.AddListener(OnAddStakesButtonClick);

        // 规则图片，不规则按钮
        btnLookCard.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.5f; // 忽略透明区域
        btnFollowStakes.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.5f;
        btnGiveUp.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.5f;
        btnAddStakes.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.5f;
        btnCompareCard.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.5f;
        tog2.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.5f;
        tog5.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.5f;
        tog10.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.5f;

        goBottomButton.SetActive(false);
        imgBanker.gameObject.SetActive(false);
        txtHint.gameObject.SetActive(false);
        goCountDown.SetActive(false);
        goCompareBtns.SetActive(false);
        btnUnReady.gameObject.SetActive(false); 

        txtStakesSum.text = "0";
        if (Models.GameModel.userDto != null) {
            imgHead.sprite = ResourcesManager.GetSprite(Models.GameModel.userDto.iconName);
            txtName.text = Models.GameModel.userDto.name;
            txtCoin.text = Models.GameModel.userDto.coin.ToString();
        }
    }

    /// <summary>
    /// 开始游戏
    /// </summary>
    private void StartGame()
    {
        txtHint.gameObject.SetActive(false);
        btnUnReady.gameObject.SetActive(false);
    }

    /// <summary>
    /// 取消准备
    /// </summary>
    private void OnUnReadyButtonClick()
    {
        btnReady.gameObject.SetActive(true);
        btnUnReady.gameObject.SetActive(false);
        txtHint.gameObject.SetActive(false);
        NetMsgCenter.Instance.SendMsg(OpCode.Match, MatchCode.UnReady_CREQ, (int)Models.GameModel.RoomType);
    }

    /// <summary>
    /// 加注按钮点击事件
    /// </summary>
    private void OnAddStakesButtonClick() {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 比牌按钮点击事件
    /// </summary>
    private void OnCompareBtnClick() {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 弃牌按钮点击事件
    /// </summary>
    private void OnGiveUpButtonClick() {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 跟注按钮点击事件
    /// </summary>
    private void OnFollowBtnClick() {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 看牌按钮点击事件
    /// </summary>
    private void OnLookCardBtnClick() {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 准备按钮点击事件
    /// </summary>
    private void OnReadyButtonClick() {
        btnReady.gameObject.SetActive(false);
        btnUnReady.gameObject.SetActive(true);
        txtHint.text = "已准备";
        txtHint.gameObject.SetActive(true);
        NetMsgCenter.Instance.SendMsg(OpCode.Match, MatchCode.Ready_CREQ, (int)Models.GameModel.RoomType);
    }

    /// <summary>
    /// 与右边玩家比牌
    /// </summary>
    private void OnCompareRightButtonClick() {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 与左边玩家比牌
    /// </summary>
    private void OnCompareLeftButtonClick() {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 成为庄家
    /// </summary>
    public void BecomeBanker() {
        imgBanker.gameObject.SetActive(true);
    }

    /// <summary>
    /// 开始下注
    /// </summary>
    public virtual void StartStakes() {
        isStartStakes = true;
        goCountDown.SetActive(true);
        txtCountDown.text = "60";
        time = 60;
    }
}
