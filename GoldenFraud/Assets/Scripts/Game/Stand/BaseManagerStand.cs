using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public abstract class BaseManagerStand : MonoBehaviour {

    protected GameManagerStand gameManager;
    public GameObject goCardPre;
    protected GameObject goCountDown;
    protected Image imgHead;
    protected Image imgBanker;
    protected Text txtStakesSum;
    protected Text txtCountDown;
    protected Transform cardPoint;

    protected StakesHint stakesHint;
    public CardType cardType; // 牌型
    public List<Card> cardList = new List<Card>(); // 自身的 3 张牌
    protected List<GameObject> goCardList = new List<GameObject>(); // 自身的 3 张牌
    protected float cardPointX = -40f; // 牌的到达位置
    protected bool isStartStakes = false; // 开始下注的标志位，用于计时
    protected float time = 60f; // 倒计时
    protected float timer = 0.0f; // 计时器
    public int stakesSum = 0; // 总下注数
    public bool isGiveUp = false; // 是否弃牌

    public abstract void CompareLose();
    public abstract void CompareWin();

    /// <summary>
    /// 成为庄家
    /// </summary>
    public void BecomeBanker() {
        imgBanker.gameObject.SetActive(true);
    }

    /// <summary>
    /// 发牌
    /// </summary>
    public void SendCard(Card card, float duration, Vector3 initPos) {
        cardList.Add(card);
        GameObject go = Instantiate(goCardPre, cardPoint); // 实例化牌
        go.GetComponent<RectTransform>().localPosition = initPos; // 设置牌的初始位置在发牌点
        go.GetComponent<RectTransform>().DOLocalMove(new Vector3(cardPointX, 0, 0), duration); // 移动到目标位置
        goCardList.Add(go);
        cardPointX += 40; // 3 张牌 40 的位置间隔
    }

    /// <summary>
    /// 排序，从大到小
    /// </summary>
    protected void SortCard() {
        // 冒泡
        for (int i = 0; i < cardList.Count - 1; i++) {
            for (int j = 0; j < cardList.Count - 1 - i; j++) {
                if (cardList[j].Weight < cardList[j + 1].Weight) {
                    Card temp = cardList[j];
                    cardList[j] = cardList[j + 1];
                    cardList[j + 1] = temp;
                }
            }
        }
    }

    /// <summary>
    /// 获取牌型
    /// </summary>
    protected void GetCardType() {
        // 235，因为前面从大到小排了序，所以反过来就是 532
        if (cardList[0].Weight == 5 && cardList[1].Weight == 3 && cardList[2].Weight == 2) {
            cardType = CardType.Max;
        }
        // 豹子，3 张一样，666
        else if (cardList[0].Weight == cardList[1].Weight && cardList[1].Weight == cardList[2].Weight) {
            cardType = CardType.Leopard;
        }
        // 顺金，是顺子，并且花色一样，765
        else if (cardList[0].Color == cardList[1].Color && cardList[1].Color == cardList[2].Color &&
            cardList[0].Weight == cardList[1].Weight + 1 && cardList[1].Weight == cardList[2].Weight + 1) {
            cardType = CardType.SGolden;
        }
        // 金花，颜色一样
        else if (cardList[0].Color == cardList[1].Color && cardList[1].Color == cardList[2].Color) {
            cardType = CardType.Golden;
        }
        // 顺子，765
        else if (cardList[0].Weight == cardList[1].Weight + 1 && cardList[1].Weight == cardList[2].Weight + 1) {
            cardType = CardType.Sequence;
        }
        // 对子，668,688（已排过序）
        else if (cardList[0].Weight == cardList[1].Weight || cardList[1].Weight == cardList[2].Weight) {
            cardType = CardType.Double;
        }
        // 单张
        else {
            cardType = CardType.Sin;
        }
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

    /// <summary>
    /// 跟注后更新金币并提示文本
    /// </summary>
    /// <param name="count"></param>
    /// <param name="str"></param>
    protected virtual void StakesAfter(int count, string str) {
        stakesHint.ShowHint(count + str);
        stakesSum += count;
        txtStakesSum.text = stakesSum.ToString();
    }
}
