using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManagerStand : MonoBehaviour {

    private SelfManagerStand selfManager;
    private LeftManagerStand leftManager;
    private RightManagerStand rightManager;
    private AudioSource audio;

    /// <summary>
    /// 左边玩家是否弃牌
    /// </summary>
    public bool LeftIsGiveUp { get { return leftManager.isGiveUp; } }
    /// <summary>
    /// 右边玩家是否弃牌
    /// </summary>
    public bool RightIsGiveUp { get { return rightManager.isGiveUp; } }

    public float sendCardDurationTime = 0.3f; // 发牌动画持续时间
    /// <summary>
    /// 牌的下标
    /// </summary>
    private int sendCardIndex = 0;
    /// <summary>
    /// 发牌游标，发到哪个人
    /// </summary>
    private int sendCardPersonIndex = 0;
    public int stakesPersonIndex = 0; // 下注游标
    private List<Card> cardList = new List<Card>(); // 牌库

    private Text txtBottomStakes;
    private Text txtTopStakes;
    private Button btnBack;
    private bool isStartStakes = false; // 是否开始下注
    private bool isNextPlayerCanStakes = true; // 下一个玩家是否可以下注
    private int lastPlayerStakes = 0;// 上一位玩家的下注数量

    private void Awake() {
        Init();
    }

    /// <summary>
    /// 设置下一位玩家可以下注
    /// </summary>
    public void SetNextPlayerStakes() {
        isNextPlayerCanStakes = true;
    }

    private void FixedUpdate() {
        if (isStartStakes) {
            if (isNextPlayerCanStakes) {
                switch (stakesPersonIndex % 3) {
                    case 0: // 自身下注
                        if (selfManager.isGiveUp == false) {
                            selfManager.StartStakes(); // 开始下注
                            isNextPlayerCanStakes = false; // 下注完后才会设为 true
                        }
                        break;
                    case 1: // 左边下注
                        if (leftManager.isGiveUp == false) {
                            leftManager.StartStakes(); // 开始下注
                            isNextPlayerCanStakes = false; // 下注完后才会设为 true
                        }
                        break;
                    case 2: // 右边下注
                        if (rightManager.isGiveUp == false) {
                            rightManager.StartStakes(); // 开始下注
                            isNextPlayerCanStakes = false; // 下注完后才会设为 true
                        }
                        break;
                    default:
                        break;
                }
                stakesPersonIndex++;
            }
        }
    }

    private void Init() {
        audio = GetComponent<AudioSource>();
        selfManager = GetComponentInChildren<SelfManagerStand>();
        leftManager = GetComponentInChildren<LeftManagerStand>();
        rightManager = GetComponentInChildren<RightManagerStand>();
        txtBottomStakes = transform.Find("Main/txtBottomStakes").GetComponent<Text>();
        txtTopStakes = transform.Find("Main/txtTopStakes").GetComponent<Text>();
        btnBack = transform.Find("Main/btnBack").GetComponent<Button>();
        btnBack.onClick.AddListener(() => {
            SceneManager.LoadScene("02 - Main");
        });

        lastPlayerStakes = Models.GameModel.BottomStakes;
        txtBottomStakes.text = Models.GameModel.BottomStakes.ToString();
        txtTopStakes.text = Models.GameModel.TopStakes.ToString();
    }

    /// <summary>
    /// 选择庄家
    /// </summary>
    public void ChooseBanker() {
        leftManager.StartChooseBanker();
        rightManager.StartChooseBanker();

        // 随机庄家
        int ran = Random.Range(0, 3);
        switch (ran) {
            case 0: // 自身成为庄家
                selfManager.BecomeBanker();
                sendCardPersonIndex = 0; // 为自己发牌
                stakesPersonIndex = 1; // 下一位玩家下注
                break;
            case 1: // 左边成为庄家
                leftManager.BecomeBanker();
                sendCardPersonIndex = 1;
                stakesPersonIndex = 2;
                break;
            case 2: // 右边成为庄家
                rightManager.BecomeBanker();
                sendCardPersonIndex = 2;
                stakesPersonIndex = 0;
                break;
            default:
                break;
        }

        EventCenter.Broadcast(EventType.Hint, "开始发牌");

        StartCoroutine(DealCard());
    }

    /// <summary>
    /// 延时发牌
    /// </summary>
    /// <returns></returns>
    IEnumerator DealCard() {
        // 1. 初始化牌
        if (cardList.Count == 0 || cardList == null || cardList.Count < 9) {
            InitCard();
            // 2. 洗牌
            Shuffle();
        }
        // 3. 发牌
        for (int i = 0; i < 9; i++) {
            audio.Play(); // 播放发牌音效
            switch (sendCardPersonIndex % 3) {
                case 0: // 自身发牌
                    selfManager.SendCard(cardList[sendCardIndex], sendCardDurationTime, new Vector3(0, 350, 0)); // 发牌
                    cardList.RemoveAt(sendCardIndex); // 从牌库移除
                    break;
                case 1: // 左边发牌
                    leftManager.SendCard(cardList[sendCardIndex], sendCardDurationTime, new Vector3(491, 3, 0));
                    cardList.RemoveAt(sendCardIndex);
                    break;
                case 2: // 右边发牌
                    rightManager.SendCard(cardList[sendCardIndex], sendCardDurationTime, new Vector3(-483, 6, 0));
                    cardList.RemoveAt(sendCardIndex);
                    break;
                default:
                    break;
            }
            sendCardIndex++;
            sendCardPersonIndex++;
            yield return new WaitForSeconds(sendCardDurationTime);
        }
        // 发牌结束
        selfManager.SendCardFinish();
        leftManager.SendCardFinish();
        rightManager.SendCardFinish();
        isStartStakes = true;
    }

    /// <summary>
    /// 初始化牌
    /// </summary>
    private void InitCard() {
        for (int weight = 2; weight <= 14; weight++) { // 2 - A
            for (int color = 0; color <= 3; color++) {
                Card card = new Card(weight, color);
                cardList.Add(card);
            }
        }
    }

    /// <summary>
    /// 洗牌
    /// </summary>
    private void Shuffle() {
        // 遍历牌库，将该张牌和牌库里随机一张牌交换
        for (int i = 0; i < cardList.Count; i++) {
            int ran = Random.Range(0, cardList.Count);
            Card temp = cardList[i];
            cardList[i] = cardList[ran];
            cardList[ran] = temp;
        }
    }

    /// <summary>
    /// 下注
    /// </summary>
    /// <param name="count"></param>
    public int Stakes(int count) {
        lastPlayerStakes += count;
        // 不能超过顶注
        if (lastPlayerStakes > Models.GameModel.TopStakes) {
            lastPlayerStakes = Models.GameModel.TopStakes;
        }
        return lastPlayerStakes;
    }

    /// <summary>
    /// 右边玩家比牌
    /// </summary>
    public void RightPlayerCompare() {
        if (selfManager.isGiveUp) { // 和左边比牌
            EventCenter.Broadcast(EventType.VSAI, (BaseManagerStand)rightManager, (BaseManagerStand)leftManager);
        }
        else { // 和 self 比牌
            EventCenter.Broadcast(EventType.VSWithSelf, (BaseManagerStand)rightManager, (BaseManagerStand)selfManager, "右边", "我");
        }
    }

    /// <summary>
    /// 左边玩家比牌
    /// </summary>
    public void LeftPlayerCompare() {
        if (selfManager.isGiveUp) { // 和右边比牌
            EventCenter.Broadcast(EventType.VSAI, (BaseManagerStand)leftManager, (BaseManagerStand)rightManager);
        }
        else { // 和 self 比牌
            EventCenter.Broadcast(EventType.VSWithSelf, (BaseManagerStand)leftManager, (BaseManagerStand)selfManager, "左边", "我");
        }
    }

    /// <summary>
    /// 自身与左边比牌
    /// </summary>
    public void SelfCompareLeft() {
        EventCenter.Broadcast(EventType.VSWithSelf, (BaseManagerStand)selfManager, (BaseManagerStand)leftManager, "我", "左边");
    }

    /// <summary>
    /// 自身与右边比牌
    /// </summary>
    public void SelfCompareRight() {
        EventCenter.Broadcast(EventType.VSWithSelf, (BaseManagerStand)selfManager, (BaseManagerStand)rightManager, "我", "右边");
    }

    /// <summary>
    /// 判断自身是否胜利
    /// </summary>
    /// <returns></returns>
    public bool IsSelfWin() {
        if (leftManager.isGiveUp && rightManager.isGiveUp) {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 判断左边是否胜利
    /// </summary>
    /// <returns></returns>
    public bool IsLeftWin() {
        if (selfManager.isGiveUp && rightManager.isGiveUp) {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 判断右边是否胜利
    /// </summary>
    /// <returns></returns>
    public bool IsRightWin() {
        if (leftManager.isGiveUp && selfManager.isGiveUp) {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 自身玩家胜利
    /// </summary>
    public void SelfWin() {
        EventCenter.Broadcast(EventType.GameOver,-leftManager.stakesSum,selfManager.stakesSum,-rightManager.stakesSum);
    }

    /// <summary>
    /// 左边玩家胜利
    /// </summary>
    public void LeftWin() {
        EventCenter.Broadcast(EventType.GameOver, leftManager.stakesSum, -selfManager.stakesSum, -rightManager.stakesSum);
    }

    /// <summary>
    /// 右边玩家胜利
    /// </summary>
    public void RightWin() {
        EventCenter.Broadcast(EventType.GameOver, -leftManager.stakesSum, -selfManager.stakesSum, rightManager.stakesSum);
    }
}
