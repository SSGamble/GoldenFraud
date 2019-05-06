using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class AutoManagerStand : BaseManagerStand {

    protected GameObject goTxtReady;
    protected GameObject goTxtGiveUp;
    protected float ranWaitStakesTime = 0; // 随机下注等待时间
    protected bool isHasStakesNum = false; // 是否有下注次数
    protected int stakesNum = 0; // 下注次数
    protected bool isComparing = false; // 是否正在比牌

    private void Awake() {
        Init();
    }

    private void FixedUpdate() {
        // 开始下注的倒计时
        if (isStartStakes) {
            if (IsWim()) {
                isStartStakes = false;
                return;
            }
            if (ranWaitStakesTime <= 0) { // 倒计时结束，下注
                PutStakes();
                isStartStakes = false;
                if (isComparing == false) {
                    goCountDown.SetActive(false);
                    gameManager.SetNextPlayerStakes();
                }
                return;
                //time = 60;
            }
            timer += Time.deltaTime;
            if (timer >= 1) {
                ranWaitStakesTime--;
                timer = 0;
                time--;
                txtCountDown.text = time.ToString();
            }
        }
    }

    private void Init() {
        gameManager = GetComponentInParent<GameManagerStand>();
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

        txtStakesSum.text = "0";
        // 随机头像
        int ran = Random.Range(0, 19);
        string name = "headIcon_" + ran;
        imgHead.sprite = ResourcesManager.GetSprite(name);
    }

    /// <summary>
    /// 开始选择庄家
    /// </summary>
    public void StartChooseBanker() {
        // 更新总下注数数据显示
        stakesSum += Models.GameModel.BottomStakes;
        txtStakesSum.text = stakesSum.ToString();
        goTxtReady.SetActive(false);
    }

    /// <summary>
    /// 判断是否胜利
    /// </summary>
    /// <returns></returns>
    public abstract bool IsWim();


    /// <summary>
    /// 发牌结束
    /// </summary>
    public virtual void SendCardFinish() {

        SortCard(); // 排序
        GetCardType(); // 获取牌型
    }

    /// <summary>
    /// 开始下注
    /// </summary>
    public override void StartStakes() {
        base.StartStakes();
        ranWaitStakesTime = Random.Range(3, 6);
    }

    /// <summary>
    /// 比牌
    /// </summary>
    public abstract void Compare();

    /// <summary>
    /// 下注
    /// </summary>
    protected void PutStakes() {
        // 如果是顺子到顺金
        if (isHasStakesNum) {
            stakesNum--;
            if (stakesNum <= 0) { // 下注次数用完
                GetPutStakesNum();
                // 比牌
                isComparing = true;
                Compare(); 
                StakesAfter(gameManager.Stakes(Random.Range(4, 6)),"看看");
                return;
            }
            int stakes = gameManager.Stakes(Random.Range(3, 6));
            StakesAfter(stakes, " 不看");
        }
        // 如果是对子，随机比牌或跟注
        else if (cardType == CardType.Double) {
            int ran = Random.Range(0, 10);
            if (ran < 5) { // 跟注
                StakesAfter(gameManager.Stakes(Random.Range(3, 6)), " 不看");
            }
            else { // 比牌
                isComparing = true;
                Compare();
                StakesAfter(gameManager.Stakes(Random.Range(4, 6)),"看看");
            }
        }
        // 单张
        else if (cardType == CardType.Sin) {
            int ran = Random.Range(0, 15);
            if (ran < 5) { // 跟注
                StakesAfter(gameManager.Stakes(Random.Range(3, 6)), " 不看");
            }
            else if (ran >= 5 && ran < 10) { // 比牌
                isComparing = true;
                Compare();
                StakesAfter(gameManager.Stakes(Random.Range(4, 6)), "看看");
            }
            else { // 弃牌
                GiveUp();
            }
        }
        // 豹子 或 Max ，一直跟注
        else if (cardType == CardType.Leopard || cardType == CardType.Max) {
            StakesAfter(gameManager.Stakes(Random.Range(4, 6)), " 不看");
        }
    }

    /// <summary>
    /// 获取下注次数
    /// </summary>
    protected void GetPutStakesNum() {
        // 如果是顺子到顺金，随机下注次数
        if ((int)cardType >= 2 && (int)cardType <= 4) {
            isHasStakesNum = true;
            stakesNum = (int)cardType * 6;
        }
    }

    /// <summary>
    /// 弃牌
    /// </summary>
    protected void GiveUp() {
        isStartStakes = false;
        goCountDown.SetActive(false);
        goTxtGiveUp.SetActive(true);
        gameManager.SetNextPlayerStakes(); // 下一个玩家
        isGiveUp = true;

        // 销毁 3 张牌
        foreach (var item in goCardList) {
            Destroy(item);
        }
    }

    public override void CompareLose() {

    }

    public override void CompareWin() {

    }
}
