using DG.Tweening;
using Protocol.Dto;
using Protocol.Dto.Fight;
using Protocol.Fight;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeftManagerOnline : MonoBehaviour {

    public GameObject goCardPre;
    protected GameObject goStakesSum;
    protected GameObject goCountDown;
    protected GameObject lookCardHint; 
    protected Text txtHint;
    protected Image imgHead;
    protected Image imgBanker;
    protected Text txtStakesSum;
    protected Text txtCountDown;
    protected Text txtName;
    protected GameObject goCoin;
    protected Text txtCoin;
    protected Transform cardPoint;

    protected StakesHint stakesHint;
    protected List<GameObject> goCardList = new List<GameObject>(); // 自身的 3 张牌
    protected float cardPointX = -40f; // 牌的到达位置
    protected bool isStartStakes = false; // 开始下注的标志位，用于计时
    protected float time = 60f; // 倒计时
    protected float timer = 0.0f; // 计时器
    public bool isRun = false; // 是否逃跑
    public bool isGiveUp = false; // 是否弃牌
    private PlayerDto playerDto;

    private void Awake() {
        EventCenter.AddListener<int>(EventType.GiveUpCardBRO, GiveUpCardBRO);
        EventCenter.AddListener<StakesDto>(EventType.PutStakesBRO, PutStakesBRO);
        EventCenter.AddListener<PlayerDto>(EventType.LeftDealCard, DealCard);
        EventCenter.AddListener<int>(EventType.LeaveFightRoom, LeaveFightRoom);
        EventCenter.AddListener<int>(EventType.StartStakes, StartStakes);
        EventCenter.AddListener<int>(EventType.LookCardBRO, LookCardBRO);
        EventCenter.AddListener(EventType.LeftBanker, Banker);
        EventCenter.AddListener(EventType.StartGame, StartGame);
        EventCenter.AddListener(EventType.RefreshUI, RefreshUI);
        Init();
    }

    private void OnDestroy() {
        EventCenter.RemoveListener<int>(EventType.GiveUpCardBRO, GiveUpCardBRO);
        EventCenter.RemoveListener<StakesDto>(EventType.PutStakesBRO, PutStakesBRO);
        EventCenter.RemoveListener<int>(EventType.LookCardBRO, LookCardBRO);
        EventCenter.RemoveListener<int>(EventType.StartStakes, StartStakes);
        EventCenter.RemoveListener<int>(EventType.LeaveFightRoom, LeaveFightRoom);
        EventCenter.RemoveListener<PlayerDto>(EventType.LeftDealCard, DealCard);
        EventCenter.RemoveListener(EventType.LeftBanker, Banker);
        EventCenter.RemoveListener(EventType.StartGame, StartGame);
        EventCenter.RemoveListener(EventType.RefreshUI, RefreshUI);
    }

    private void FixedUpdate() {
        if (isStartStakes) {
            if (time<=0) {
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
        if (giveUpUserId == Models.GameModel.MatchRoomDto.LeftPlayerId) {
            goCountDown.SetActive(false);
            isStartStakes = false;
            txtHint.text = "已弃牌";
            txtHint.gameObject.SetActive(true);
            isGiveUp = true;
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
        if (dto.userId == Models.GameModel.MatchRoomDto.LeftPlayerId) {
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
        isStartStakes = false;
    }

    /// <summary>
    /// 有玩家看牌的服务器广播
    /// </summary>
    /// <param name="arg"></param>
    private void LookCardBRO(int userId) {
        if (userId == Models.GameModel.MatchRoomDto.LeftPlayerId) {
            lookCardHint.SetActive(true);
        }
    }

    /// <summary>
    /// 开始下注
    /// </summary>
    /// <param name="arg"></param>
    private void StartStakes(int userId) {
        if (userId == Models.GameModel.MatchRoomDto.LeftPlayerId) {
            time = 60;
            goCountDown.SetActive(true);
            txtCountDown.text = "60";
            isStartStakes = true;
        }
        else {
            goCountDown.SetActive(false);
            isStartStakes = false;
        }
    }

    /// <summary>
    /// 有玩家离开了服务器发来的响应
    /// </summary>
    /// <param name="arg"></param>
    private void LeaveFightRoom(int leaveUserID) {
        if (leaveUserID==Models.GameModel.MatchRoomDto.LeftPlayerId) {
            HintObj();
            txtHint.text = "逃跑了";
            txtHint.gameObject.SetActive(true);
            isRun = true;
            // 销毁牌
            foreach (var item in goCardList) {
                Destroy(item);
            }
        }
    }

    /// <summary>
    /// 发牌
    /// </summary>
    /// <param name="arg"></param>
    private void DealCard(PlayerDto player) {
        playerDto = player;
        goCardList.Clear();
        foreach (var card in player.cardLidt) {
            SendCard(0.3f, new Vector3(491, 3, 0));
        }
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
    private void StartGame() {
        txtStakesSum.text = playerDto.stakesSum.ToString();
        txtHint.gameObject.SetActive(false);
    }

    /// <summary>
    /// 当有玩家进来或离开，自己进来时，刷新 UI
    /// </summary>
    private void RefreshUI() {
        MatchRoomDto room = Models.GameModel.MatchRoomDto;

        if (room.LeftPlayerId != -1) // 左边有人
        {
            UserDto userDto = room.userIdUserDto[room.LeftPlayerId];
            imgHead.gameObject.SetActive(true);
            imgHead.sprite = ResourcesManager.GetSprite(userDto.iconName);
            goCoin.SetActive(true);
            txtCoin.text = userDto.coin.ToString();
            goStakesSum.SetActive(true);

            txtName.gameObject.SetActive(true);
            txtName.text = userDto.name;

            // 左边玩家在准备中
            if (room.readyUserList.Contains(room.LeftPlayerId)) {
                txtHint.text = "已准备";
                txtHint.gameObject.SetActive(true);
            }
            else {
                txtHint.gameObject.SetActive(false);
            }
        }
        else {
            txtHint.gameObject.SetActive(false);
            imgHead.gameObject.SetActive(false);
            goCoin.SetActive(false);
            goStakesSum.SetActive(false);
            txtName.gameObject.SetActive(false);
        }
    }

    private void Init() {
        stakesHint = transform.Find("txtStakesHint").GetComponent<StakesHint>();

        goStakesSum = transform.Find("StakesSum").gameObject;
        txtStakesSum = transform.Find("StakesSum/txtStakesSum").GetComponent<Text>();
        goCoin = transform.Find("Coin").gameObject;
        lookCardHint = transform.Find("LookCardHint").gameObject;
        txtCoin = transform.Find("Coin/txtCoin").GetComponent<Text>();
        goCountDown = transform.Find("goCountDown").gameObject;
        txtCountDown = transform.Find("goCountDown/txtCountDown").GetComponent<Text>();
        txtHint = transform.Find("txtHint").GetComponent<Text>();
        imgBanker = transform.Find("imgBanker").GetComponent<Image>();
        imgHead = transform.Find("imgHead").GetComponent<Image>();
        txtName = transform.Find("txtName").GetComponent<Text>();
        cardPoint = transform.Find("cardPoint");

        txtStakesSum.text = "0";

        HintObj();
    }

    private void HintObj() {
        imgBanker.gameObject.SetActive(false);
        goCountDown.SetActive(false);
        txtHint.gameObject.SetActive(false);
        goStakesSum.SetActive(false);
        goCoin.SetActive(false);
        txtName.gameObject.SetActive(false);
        lookCardHint.SetActive(false);
        imgHead.gameObject.SetActive(false);
    }
}
