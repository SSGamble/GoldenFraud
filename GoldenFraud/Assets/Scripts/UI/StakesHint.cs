using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class StakesHint : MonoBehaviour {

    private Text txtHint;
    private float hintShowTime = 1.5f;

    private void Awake() {
        txtHint = GetComponent<Text>();
    }

    public void ShowHint(string str) {
        txtHint.text = str;
        txtHint.DOFade(1, 0.3f).OnComplete(() => {
            StartCoroutine(Dealy());
        });
    }

    /// <summary>
    /// 延时隐藏
    /// </summary>
    /// <returns></returns>
    IEnumerator Dealy() {
        yield return new WaitForSeconds(hintShowTime);
        txtHint.DOFade(0, 0.3f);
    }
}
