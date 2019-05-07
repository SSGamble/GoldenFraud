using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Protocol.Dto.Fight;

public class VSPanelOnline : MonoBehaviour {

    [System.Serializable]
    public class Player {
        public Text txtName;
        public Image[] imgCardArr;
    }

    public Player win;
    public Player lose;

    private void Awake() {
        EventCenter.AddListener<CompareResultDto>(EventType.CompareCardBRO, Compare);
    }

    private void OnDestroy() {
        EventCenter.RemoveListener<CompareResultDto>(EventType.CompareCardBRO, Compare);
    }

    private void Compare(CompareResultDto dto) {
        transform.DOScale(Vector3.one, 0.3f).OnComplete(()=> {
            StartCoroutine(Delay());
        });
        // 显示牌
        for (int i = 0; i < dto.winDto.cardLidt.Count; i++) {
            win.imgCardArr[i].sprite = ResourcesManager.LoadCardSprite(dto.winDto.cardLidt[i].Name);

        }
        win.txtName.text = dto.winDto.name;
        for (int i = 0; i < dto.loseDto.cardLidt.Count; i++) {
            lose.imgCardArr[i].sprite = ResourcesManager.LoadCardSprite(dto.loseDto.cardLidt[i].Name);

        }
        lose.txtName.text = dto.loseDto.name;
    }

    public float dealyTime = 4f;

    IEnumerator Delay() {
        yield return new WaitForSeconds(dealyTime);
        transform.DOScale(Vector3.zero, 0.3f);
    }
}
