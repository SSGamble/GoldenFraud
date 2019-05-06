using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class VSPanel_Stand : MonoBehaviour {

    [System.Serializable]
    public class Player {
        public Text txtName;
        public Image[] imgCardArr;
        public Image imgLose;
        public Image imgWin;
    }

    public Player comparePlayer;
    public Player comparedPlayer;
    public BaseManagerStand c1;
    public BaseManagerStand c2;
    public float dealyTime = 4f;

    private void Awake() {
        EventCenter.AddListener<BaseManagerStand, BaseManagerStand>(EventType.VSAI, CompareCard);
        EventCenter.AddListener<BaseManagerStand, BaseManagerStand, string, string>(EventType.VSWithSelf, VSWithSelf);
    }

    private void OnDestroy() {
        EventCenter.RemoveListener<BaseManagerStand, BaseManagerStand>(EventType.VSAI, CompareCard);
        EventCenter.RemoveListener<BaseManagerStand, BaseManagerStand, string, string>(EventType.VSWithSelf, VSWithSelf);

    }

    IEnumerator Delay() {
        yield return new WaitForSeconds(dealyTime);
        transform.DOScale(Vector3.zero, 0.3f);
    }

    IEnumerator C1Win() {
        yield return new WaitForSeconds(dealyTime);
        c1.CompareWin();
        c2.CompareLose();
    }

    IEnumerator C1Lose() {
        yield return new WaitForSeconds(dealyTime);
        c1.CompareLose();
        c2.CompareWin();
    }

    /// <summary>
    /// AI 和 Self 比牌，与 AI 和 AI 相比，多了 UI 的处理
    /// </summary>
    private void VSWithSelf(BaseManagerStand compare, BaseManagerStand compared, string compareName, string comparedName) {

        transform.DOScale(Vector3.one, 0.3f).OnComplete(()=> {
            StartCoroutine(Delay());
        });

        comparePlayer.imgLose.gameObject.SetActive(false);
        comparePlayer.imgWin.gameObject.SetActive(false);
        comparedPlayer.imgLose.gameObject.SetActive(false);
        comparedPlayer.imgWin.gameObject.SetActive(false);

        comparePlayer.txtName.text = compareName;
        comparedPlayer.txtName.text = comparedName;

        // 显示各自的 3 张牌
        for (int i = 0; i < compare.cardList.Count; i++) {
            string cardName = "card_" + compare.cardList[i].Color + "_" + compare.cardList[i].Weight;
            comparePlayer.imgCardArr[i].sprite = ResourcesManager.LoadCardSprite(cardName);
        }
        for (int i = 0; i < compared.cardList.Count; i++) {
            string cardName = "card_" + compared.cardList[i].Color + "_" + compared.cardList[i].Weight;
            comparedPlayer.imgCardArr[i].sprite = ResourcesManager.LoadCardSprite(cardName);
        }

        CompareCard(compare, compared);
    }

    /// <summary>
    /// 比牌的逻辑算法
    /// </summary>
    private void CompareCard(BaseManagerStand c1, BaseManagerStand c2) {
        this.c1 = c1;
        this.c2 = c2;
        bool c1Win = false;
        // 牌型比较
        if (c1.cardType > c2.cardType) { // c1 胜
            c1Win = true;
        }
        else if (c1.cardType == c2.cardType) { // 牌型相同
            // 单张
            if (c1.cardType == CardType.Sin) {
                c1Win = CompareSinCard(c1, c2);
            }

            // 对子 662 663 / 766 866 / 662 966
            if (c1.cardType == CardType.Double) {
                int c1Double = 0, c1Sin = 0, c2Double = 0, c2Sin = 0;
                // c1
                if (c1.cardList[0].Weight == c1.cardList[1].Weight) { // 对子在前
                    c1Double = c1.cardList[0].Weight;
                    c1Sin = c1.cardList[2].Weight;
                }
                if (c1.cardList[1].Weight == c1.cardList[2].Weight) { // 对子在后
                    c1Double = c1.cardList[1].Weight;
                    c1Sin = c1.cardList[0].Weight;
                }
                // c2
                if (c2.cardList[0].Weight == c2.cardList[1].Weight) { // 对子在前
                    c2Double = c2.cardList[0].Weight;
                    c2Sin = c2.cardList[2].Weight;
                }
                if (c2.cardList[1].Weight == c2.cardList[2].Weight) { // 对子在后
                    c2Double = c2.cardList[1].Weight;
                    c2Sin = c2.cardList[0].Weight;
                }
                // 比较对子
                if (c1Double > c2Double) {
                    c1Win = true;
                }
                else if (c1Double == c2Double) {
                    // 比较单张
                    if (c1Sin > c2Sin) {
                        c1Win = true;
                    }
                    else {
                        c1Win = false;
                    }
                }
                else {
                    c1Win = false;
                }
            }

            // 顺子,顺金，豹子，都直接比较 3 张牌加起来的值，谁大谁赢
            if (c1.cardType == CardType.Sequence || c1.cardType == CardType.SGolden || c1.cardType == CardType.Leopard) {
                // 获取和
                int c1Sum = 0, c2Sum = 0;
                for (int i = 0; i < c1.cardList.Count; i++) {
                    c1Sum += c1.cardList[i].Weight;
                }
                for (int i = 0; i < c2.cardList.Count; i++) {
                    c2Sum += c2.cardList[i].Weight;
                }
                // 比较和
                if (c1Sum > c2Sum) {
                    c1Win = true;
                }
                else {
                    c1Win = false;
                }
            }

            // 金花
            if (c1.cardType == CardType.SGolden) {
                c1Win = CompareSinCard(c1, c2);
            }

            // Max 235
            if (c1.cardType == CardType.Max) {
                c1Win = false;
            }

        }
        else { // c1 输
            c1Win = false;
        }

        if (c1Win) {
            StartCoroutine(C1Win());
            comparePlayer.imgWin.gameObject.SetActive(true);
            comparedPlayer.imgLose.gameObject.SetActive(true);
        }
        else {
            StartCoroutine(C1Lose());
            comparePlayer.imgLose.gameObject.SetActive(true);
            comparedPlayer.imgWin.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 按单张比较牌
    /// </summary>
    /// <param name="c1"></param>
    /// <param name="c2"></param>
    /// <returns></returns>
    private bool CompareSinCard(BaseManagerStand c1, BaseManagerStand c2) {
        bool c1Win = false;
        // 第一张
        if (c1.cardList[0].Weight > c2.cardList[0].Weight) {
            c1Win = true;
        }
        else if (c1.cardList[0].Weight == c2.cardList[0].Weight) {
            // 第 2 张
            if (c1.cardList[1].Weight > c2.cardList[1].Weight) {
                c1Win = true;
            }
            else if (c1.cardList[1].Weight == c2.cardList[1].Weight) {
                // 第 3 张
                if (c1.cardList[2].Weight > c2.cardList[2].Weight) {
                    c1Win = true;
                }
                else {
                    c1Win = false;
                }
            }
            else {
                c1Win = false;
            }
        }
        else {
            c1Win = false;
        }
        return c1Win;
    }
}
