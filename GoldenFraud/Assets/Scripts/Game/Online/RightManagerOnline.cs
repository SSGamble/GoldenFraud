using Protocol.Dto;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RightManagerOnline : MonoBehaviour {

    public GameObject goCardPre;
    protected GameObject goStakesSum;
    protected GameObject goCountDown;
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

    private void Awake()
    {
        EventCenter.AddListener(EventType.StartGame, StartGame);
        EventCenter.AddListener(EventType.RefreshUI, RefreshUI);
        Init();
    }

    private void OnDestroy()
    {
        EventCenter.RemoveListener(EventType.StartGame, StartGame);
        EventCenter.RemoveListener(EventType.RefreshUI, RefreshUI);
    }

    private void StartGame()
    {
        txtHint.gameObject.SetActive(false);
    }

    private void Init()
    {
        stakesHint = transform.Find("txtStakesHint").GetComponent<StakesHint>();

        goStakesSum = transform.Find("StakesSum").gameObject;
        txtStakesSum = transform.Find("StakesSum/txtStakesSum").GetComponent<Text>();
        goCoin = transform.Find("Coin").gameObject;
        txtCoin = transform.Find("Coin/txtCoin").GetComponent<Text>();
        goCountDown = transform.Find("goCountDown").gameObject;
        txtCountDown = transform.Find("goCountDown/txtCountDown").GetComponent<Text>();
        txtHint = transform.Find("txtHint").GetComponent<Text>();
        imgBanker = transform.Find("imgBanker").GetComponent<Image>();
        imgHead = transform.Find("imgHead").GetComponent<Image>();
        txtName = transform.Find("txtName").GetComponent<Text>();
        cardPoint = transform.Find("cardPoint");

        imgBanker.gameObject.SetActive(false);
        goCountDown.SetActive(false);
        txtHint.gameObject.SetActive(false);
        goStakesSum.SetActive(false);
        goCoin.SetActive(false);
        txtName.gameObject.SetActive(false);
        imgHead.gameObject.SetActive(false);

        txtStakesSum.text = "0";
    }

    /// <summary>
    /// 当有玩家进来或离开，自己进来时，刷新 UI
    /// </summary>
    private void RefreshUI()
    {
        MatchRoomDto room = Models.GameModel.MatchRoomDto;

        if (room.RightPlayerId != -1) // 左边有人
        {
            UserDto userDto = room.userIdUserDto[room.RightPlayerId];
            imgHead.gameObject.SetActive(true);
            imgHead.sprite = ResourcesManager.GetSprite(userDto.iconName);
            goCoin.SetActive(true);
            txtCoin.text = userDto.coin.ToString();
            goStakesSum.SetActive(true);

            txtName.gameObject.SetActive(true);
            txtName.text = userDto.name;

            // 右边玩家在准备中
            if (room.readyUserList.Contains(room.RightPlayerId))
            {
                txtHint.text = "已准备";
                txtHint.gameObject.SetActive(true);
            }
            else
            {
                txtHint.gameObject.SetActive(false);
            }
        }
        else
        {
            txtHint.gameObject.SetActive(false);
            imgHead.gameObject.SetActive(false);
            goCoin.SetActive(false);
            goStakesSum.SetActive(false);
            txtName.gameObject.SetActive(false);
        }
    }
}
