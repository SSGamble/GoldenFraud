using System;
using System.Collections;
using System.Collections.Generic;
using Protocol.Code;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// 单机 - 玩家管理
/// </summary>
public class SelfManagerStand : BaseManagerStand {

    private AudioSource audio;
    public AudioClip startClip;
    public AudioClip giveUpClip;

    private GameObject goBottomButton;
    private GameObject goTxtGiveUp;
    private GameObject goCompareBtns;
    private Text txtName;
    private Text txtCoin;
    private Button btnReady;
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


    private void Awake() {
        EventCenter.AddListener<int>(EventType.UpdateCoin, UpdateCoin);
        Init();
    }

    private void FixedUpdate() {
        // 选中的倍数变黑
        if (tog2.isOn) {
            tog2.GetComponent<Image>().color = Color.gray;
            tog5.GetComponent<Image>().color = Color.white;
            tog10.GetComponent<Image>().color = Color.white;
        }
        if (tog5.isOn) {
            tog5.GetComponent<Image>().color = Color.gray;
            tog2.GetComponent<Image>().color = Color.white;
            tog10.GetComponent<Image>().color = Color.white;
        }
        if (tog10.isOn) {
            tog10.GetComponent<Image>().color = Color.gray;
            tog5.GetComponent<Image>().color = Color.white;
            tog2.GetComponent<Image>().color = Color.white;
        }
        // 开始下注的倒计时
        if (isStartStakes) {
            if (gameManager.IsSelfWin()) {
                gameManager.SelfWin();
                isStartStakes = false;
                return;
            }
            if (time <= 0) {
                OnFollowBtnClick(); // 默认跟注
                time = 60;
            }
            timer += Time.deltaTime;
            if (timer >= 1) {
                timer = 0;
                time--;
                txtCountDown.text = time.ToString();
            }
        }
    }

    private void OnDestroy() {
        EventCenter.RemoveListener<int>(EventType.UpdateCoin, UpdateCoin);

    }

    private void Init() {
        audio = GetComponent<AudioSource>();
        gameManager = GetComponentInParent<GameManagerStand>(); // 挂载在 Canvas 上

        goCompareBtns = transform.Find("goCompareBtns").gameObject;
        btnCompareLeft = goCompareBtns.transform.Find("btnCompareLeft").GetComponent<Button>();
        btnCompareLeft.onClick.AddListener(OnCompareLeftButtonClick);
        btnCompareRight = goCompareBtns.transform.Find("btnCompareRight").GetComponent<Button>();
        btnCompareRight.onClick.AddListener(OnCompareRightButtonClick);
        goBottomButton = transform.Find("goBottomButton").gameObject;
        goCountDown = transform.Find("goCountDown").gameObject;
        goTxtGiveUp = transform.Find("goTxtGiveUp").gameObject;
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
        btnReady.onClick.AddListener(OnReadButtonClick);
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
        goTxtGiveUp.SetActive(false);
        goCountDown.SetActive(false);
        goCompareBtns.SetActive(false);

        txtStakesSum.text = "0";
        if (Models.GameModel.userDto != null) {
            imgHead.sprite = ResourcesManager.GetSprite(Models.GameModel.userDto.iconName);
            txtName.text = Models.GameModel.userDto.name;
            txtCoin.text = Models.GameModel.userDto.coin.ToString();
        }
    }

    /// <summary>
    /// 与左边比牌按钮点击事件
    /// </summary>
    private void OnCompareLeftButtonClick() {
        Debug.Log("left");
        gameManager.SelfCompareLeft();
        SetBtnInteractable(false);
    }

    /// <summary>
    /// 与右边比牌按钮点击事件
    /// </summary>
    private void OnCompareRightButtonClick() {
        Debug.Log("right");
        gameManager.SelfCompareRight();
        SetBtnInteractable(false);
    }

    /// <summary>
    /// 比牌按钮点击事件
    /// </summary>
    private void OnCompareBtnClick() {
        goCompareBtns.SetActive(true);
        if (gameManager.LeftIsGiveUp) {
            btnCompareLeft.gameObject.SetActive(false);
        }
        if (gameManager.RightIsGiveUp) {
            btnCompareRight.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 加注按钮点击事件
    /// </summary>
    private void OnAddStakesButtonClick() {
        // 加注，倍数
        if (tog2.isOn) {
            StakesAfter(gameManager.Stakes(gameManager.Stakes(0) * 1), "不看");
        }
        if (tog5.isOn) {
            StakesAfter(gameManager.Stakes(gameManager.Stakes(0) * 4), "不看");
        }
        if (tog10.isOn) {
            StakesAfter(gameManager.Stakes(gameManager.Stakes(0) * 9), "不看");
        }
        isStartStakes = false;
        goCountDown.SetActive(false);
        SetBtnInteractable(false);
        gameManager.SetNextPlayerStakes();
        goCompareBtns.SetActive(false);

    }

    /// <summary>
    /// 跟注按钮点击事件
    /// </summary>
    private void OnFollowBtnClick() {
        int stakes = gameManager.Stakes(0);
        gameManager.SetNextPlayerStakes();
        isStartStakes = false;
        goCountDown.SetActive(false);
        SetBtnInteractable(false);
        goCompareBtns.SetActive(false);
        StakesAfter(stakes," 不看");
    }

    /// <summary>
    /// 跟注后更新金币并提示文本
    /// </summary>
    /// <param name="count"></param>
    /// <param name="str"></param>
    protected override void StakesAfter(int count, string str) {
        base.StakesAfter(count, str);
        if (NetMsgCenter.Instance != null) {
            NetMsgCenter.Instance.SendMsg(OpCode.Account, AccountCode.UpdateCoin_CREQ, -count);
        }
    }

    /// <summary>
    /// 开始下注
    /// </summary>
    public override void StartStakes() {
        base.StartStakes();
        SetBtnInteractable(true);
    }

    /// <summary>
    /// 看牌按钮点击事件
    /// </summary>
    private void OnLookCardBtnClick() {
        btnLookCard.interactable = false;
        for (int i = 0; i < cardList.Count; i++) {
            string cardName = "card_" + cardList[i].Color.ToString() + "_" + cardList[i].Weight.ToString();
            goCardList[i].GetComponent<Image>().sprite = ResourcesManager.LoadCardSprite(cardName);
        }
    }

    /// <summary>
    /// 准备按钮点击事件
    /// </summary>
    private void OnReadButtonClick() {
        audio.clip = startClip;
        audio.Play();
        // 更新总下注数数据显示
        stakesSum += Models.GameModel.BottomStakes;
        txtStakesSum.text = stakesSum.ToString();
        // 通知服务器减少金币
        if (NetMsgCenter.Instance != null) {
            NetMsgCenter.Instance.SendMsg(OpCode.Account, AccountCode.UpdateCoin_CREQ, -Models.GameModel.BottomStakes);
        }

        btnReady.gameObject.SetActive(false);
        gameManager.ChooseBanker();
    }

    /// <summary>
    /// 金币数量更新后的调用
    /// </summary>
    /// <param name="arg"></param>
    private void UpdateCoin(int arg) {
        txtCoin.text = arg.ToString();
    }

    /// <summary>
    /// 设置底部按钮是否可以交互
    /// </summary>
    private void SetBtnInteractable(bool value) {
        btnFollowStakes.interactable = value; // 设置按钮是否交互
        btnAddStakes.interactable = value;
        btnCompareCard.interactable = value;
        btnGiveUp.interactable = value;
        tog2.interactable = value;
        tog5.interactable = value;
        tog10.interactable = value;
    }

    /// <summary>
    /// 发牌结束
    /// </summary>
    public void SendCardFinish() {
        SetBtnInteractable(false); // 设置底部按钮不可交互
        goBottomButton.SetActive(true);

        SortCard(); // 排序
        GetCardType(); // 获取牌型
        print("自身牌型" + cardType);
    }

    /// <summary>
    /// 比牌输
    /// </summary>
    public override void CompareLose() {
        OnGiveUpButtonClick(); // 弃牌
    }

    /// <summary>
    /// 比牌赢
    /// </summary>
    public override void CompareWin() {
        isStartStakes = false;
        goCountDown.SetActive(false);
        gameManager.stakesPersonIndex = 0; // 下注游标，自身赢了，可以再次下注
        gameManager.SetNextPlayerStakes();
    }

    /// <summary>
    /// 弃牌按钮点击事件
    /// </summary>
    private void OnGiveUpButtonClick() {
        audio.clip = giveUpClip;
        audio.Play();
        isStartStakes = false;
        goBottomButton.SetActive(false);
        goCountDown.SetActive(false);
        goCompareBtns.SetActive(false);
        isGiveUp = true;
        goTxtGiveUp.SetActive(true);
        foreach (var item in goCardList) {
            Destroy(item);
        }
        gameManager.SetNextPlayerStakes();
    }
}
