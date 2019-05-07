using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Protocol.Code;
using Protocol.Dto;
using Protocol.Dto.Fight;
using Protocol.Fight;
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
    protected List<CardDto> cardList = new List<CardDto>(); // 牌的集合
    protected float cardPointX = -40f; // 牌的到达位置
    protected bool isStartStakes = false; // 开始下注的标志位，用于计时
    protected float time = 60f; // 倒计时
    protected float timer = 0.0f; // 计时器
    private PlayerDto playerDto;

    private GameManagerOnline gameManager;

    private void Awake() {
        EventCenter.AddListener<StakesDto>(EventType.PutStakesBRO, PutStakesBRO);
        EventCenter.AddListener<int>(EventType.StartStakes, StartStakes);
        EventCenter.AddListener<int>(EventType.GiveUpCardBRO, GiveUpCardBRO);
        EventCenter.AddListener<PlayerDto>(EventType.SelfDealCard, DealCard);
        EventCenter.AddListener(EventType.SelfBanker, Banker);
        EventCenter.AddListener(EventType.StartGame, StartGame);
        Init();
    }


    private void OnDestroy() {
        EventCenter.RemoveListener<StakesDto>(EventType.PutStakesBRO, PutStakesBRO);
        EventCenter.RemoveListener<int>(EventType.StartStakes, StartStakes);
        EventCenter.RemoveListener<int>(EventType.GiveUpCardBRO, GiveUpCardBRO);
        EventCenter.RemoveListener<PlayerDto>(EventType.SelfDealCard, DealCard);
        EventCenter.RemoveListener (EventType.SelfBanker, Banker);
        EventCenter.RemoveListener(EventType.StartGame, StartGame);
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
        if (isStartStakes) {
            if (time <= 0) {
                isStartStakes = false;
                time = 60;
                timer = 0;
                return;
            }
            timer += Time.deltaTime;
            if (timer >= 1) {
                timer = 0;
                time--;
                txtCountDown.text = time.ToString();
            }
        }
    }

    /// <summary>
    /// 有玩家弃牌的广播
    /// </summary>
    /// <param name="giveupUserid"></param>
    private void GiveUpCardBRO(int giveUpUserId) {
        // 自身弃牌
        if (giveUpUserId == Models.GameModel.userDto.id) {
            goBottomButton.SetActive(false);
            goCountDown.SetActive(false);
            isStartStakes = false;
            txtHint.text = "已弃牌";
            txtHint.gameObject.SetActive(true);
            goCompareBtns.SetActive(false);
            foreach (var item in goCardList) {
                Destroy(item);
            }
        }
    }

    /// <summary>
    /// 有玩家下注的服务器广播
    /// </summary>
    /// <param name="arg"></param>
    private void PutStakesBRO(StakesDto dto) {
        // 自身
        if (dto.userId == Models.GameModel.userDto.id) {
            txtCoin.text = dto.remainCoin.ToString();
            if (dto.stakesType == StakesDto.StakesType.NoLook) {
                stakesHint.ShowHint(dto.stakesCount + "不看");
                txtStakesSum.text = dto.stakesSum.ToString();
            }
            else {
                stakesHint.ShowHint(dto.stakesCount + "看看");
                txtStakesSum.text = dto.stakesSum.ToString();
            }
        }
        goCountDown.SetActive(false);
        SetBtnInteractable(false);
        isStartStakes = false;
        goCompareBtns.SetActive(false);
    }

    /// <summary>
    /// 开始下注
    /// </summary>
    /// <param name="arg"></param>
    private void StartStakes(int userId) {
        if (userId == Models.GameModel.userDto.id) {
            time = 60;
            goCountDown.SetActive(true);
            txtCountDown.text = "60";
            isStartStakes = true;
            SetBtnInteractable(true);
        }
        else {
            goCountDown.SetActive(false);
            isStartStakes = false;
            SetBtnInteractable(false);
        }
    }

    private void Init() {
        gameManager = GetComponentInParent<GameManagerOnline>();
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
    /// 发牌完成后
    /// </summary>
    private void DealCardFinished() {
        goBottomButton.SetActive(true);
        SetBtnInteractable(false);
    }

    /// <summary>
    /// 发牌
    /// </summary>
    /// <param name="arg"></param>
    private void DealCard(PlayerDto player) {
        playerDto = player;
        goCardList.Clear();
        this.cardList = player.cardLidt;
        foreach (var card in player.cardLidt) {
            SendCard(0.3f, new Vector3(0, 350, 0));
        }
        DealCardFinished();
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
    /// 发牌
    /// </summary>
    public void SendCard(float duration, Vector3 initPos) {
        GameObject go = Instantiate(goCardPre, cardPoint); // 实例化牌
        go.GetComponent<RectTransform>().localPosition = initPos; // 设置牌的初始位置在发牌点
        go.GetComponent<RectTransform>().DOLocalMove(new Vector3(cardPointX, 0, 0), duration); // 移动到目标位置
        goCardList.Add(go);
        cardPointX += 40; // 3 张牌 40 的位置间隔
    }


    /// <summary>
    /// 成为庄家
    /// </summary>
    private void Banker() {
        imgBanker.gameObject.SetActive(true);
    }

    /// <summary>
    /// 开始游戏
    /// </summary>
    private void StartGame()
    {
        txtStakesSum.text = playerDto.stakesSum.ToString();
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
        if (tog2.isOn) {
            NetMsgCenter.Instance.SendMsg(OpCode.Fight, FightCode.AddStakes_CREQ, 2);
        }
        if (tog5.isOn) {
            NetMsgCenter.Instance.SendMsg(OpCode.Fight, FightCode.AddStakes_CREQ, 5);
        }
        if (tog10.isOn) {
            NetMsgCenter.Instance.SendMsg(OpCode.Fight, FightCode.AddStakes_CREQ, 10);
        }
    }

    /// <summary>
    /// 比牌按钮点击事件
    /// </summary>
    private void OnCompareBtnClick() {
        goCompareBtns.SetActive(true);
        if (gameManager.LeftIsGiveUp||gameManager.LeftIsLeave) {
            btnCompareLeft.gameObject.SetActive(false);
        }
        if (gameManager.RightIsGiveUp || gameManager.RightIsLeave) {
            btnCompareRight.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 弃牌按钮点击事件
    /// </summary>
    private void OnGiveUpButtonClick() {
        NetMsgCenter.Instance.SendMsg(OpCode.Fight, FightCode.GiveUpCard_CREQ, null);

    }

    /// <summary>
    /// 跟注按钮点击事件
    /// </summary>
    private void OnFollowBtnClick() {
        NetMsgCenter.Instance.SendMsg(OpCode.Fight, FightCode.Follow_CREQ, null);
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
        NetMsgCenter.Instance.SendMsg(OpCode.Fight, FightCode.LookCard_CREQ, null);
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
        btnCompareLeft.gameObject.SetActive(false);
        NetMsgCenter.Instance.SendMsg(OpCode.Fight, FightCode.CompareCard_CREQ, Models.GameModel.MatchRoomDto.RightPlayerId);
    }

    /// <summary>
    /// 与左边玩家比牌
    /// </summary>
    private void OnCompareLeftButtonClick() {
        btnCompareRight.gameObject.SetActive(false);
        NetMsgCenter.Instance.SendMsg(OpCode.Fight, FightCode.CompareCard_CREQ, Models.GameModel.MatchRoomDto.LeftPlayerId);
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
