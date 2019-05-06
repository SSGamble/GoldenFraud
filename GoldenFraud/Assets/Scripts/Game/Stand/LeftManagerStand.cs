using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LeftManagerStand : AutoManagerStand {

    /// <summary>
    /// 比牌
    /// </summary>
    public override void Compare() {
        gameManager.LeftPlayerCompare();
    }

    public override void SendCardFinish() {
        base.SendCardFinish();
        print("Left 牌型" + cardType);
    }

    public override bool IsWim() {
        if (gameManager.IsLeftWin()) {
            gameManager.LeftWin();
            return true;
        }
        return false;
    }

    public override void CompareLose() {
        base.CompareLose();
        // TODO
        GiveUp(); // 自己添加
    }

    public override void CompareWin() {
        base.CompareWin();
        isStartStakes = false;
        goCountDown.SetActive(false);
        gameManager.stakesPersonIndex = 1; // 下注游标
        gameManager.SetNextPlayerStakes();
        isComparing = false;
    }
}