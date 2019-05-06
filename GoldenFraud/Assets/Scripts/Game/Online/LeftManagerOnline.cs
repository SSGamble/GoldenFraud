using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeftManagerOnline : MonoBehaviour {

    public GameObject goCardPre;
    protected GameObject goCountDown;
    protected GameObject goTxtReady;
    protected GameObject goTxtGiveUp;
    protected Image imgHead;
    protected Image imgBanker;
    protected Text txtStakesSum;
    protected Text txtCountDown;
    protected Transform cardPoint;

    protected StakesHint stakesHint;
    protected List<GameObject> goCardList = new List<GameObject>(); // 自身的 3 张牌
    protected float cardPointX = -40f; // 牌的到达位置
    protected bool isStartStakes = false; // 开始下注的标志位，用于计时
    protected float time = 60f; // 倒计时
    protected float timer = 0.0f; // 计时器

    private void Awake() {
        Init();
    }

    private void Init() {
        stakesHint = transform.Find("txtStakesHint").GetComponent<StakesHint>();

        goCountDown = transform.Find("goCountDown").gameObject;
        goTxtReady = transform.Find("goTxtReady").gameObject;
        goTxtGiveUp = transform.Find("goTxtGiveUp").gameObject;
        imgBanker = transform.Find("imgBanker").GetComponent<Image>();
        imgHead = transform.Find("imgHead").GetComponent<Image>();
        txtStakesSum = transform.Find("StakesSum/txtStakesSum").GetComponent<Text>();
        txtCountDown = transform.Find("goCountDown/txtCountDown").GetComponent<Text>();
        cardPoint = transform.Find("cardPoint");

        imgBanker.gameObject.SetActive(false);
        goCountDown.SetActive(false);
        goTxtGiveUp.SetActive(false);
        goTxtReady.SetActive(false);

        txtStakesSum.text = "0";
    }
}
