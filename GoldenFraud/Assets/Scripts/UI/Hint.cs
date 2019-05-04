using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// 提示文字
/// </summary>
public class Hint : MonoBehaviour {

    private Text txtHint;
    private float hintShowTime = 2f;

    private void Awake() {
        EventCenter.AddListener<string>(EventType.Hint, ShowHint);
        txtHint = GetComponent<Text>();
    }

    private void OnDestroy() {
        EventCenter.RemoveListener<string>(EventType.Hint, ShowHint);
    }

    private void ShowHint(string str) {
        txtHint.text = str;
        txtHint.transform.DOLocalMoveY(-155+540, 0.3f);
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
        txtHint.transform.DOLocalMoveY(540, 0.3f);
    }
}
