using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Protocol.Code;
using Protocol.Dto.Fight;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverPanelOnline : MonoBehaviour {

    [System.Serializable]
    public class Player {
        public Text txtName;
        public Text txtCoin;
    }

    public Player lose1;
    public Player lose2;
    public Player win;

    private Button btnAgain;
    private Button btnHome;

    private AudioSource audio;
    public AudioClip winClip;
    public AudioClip loseClip;

    private void Awake() {
        EventCenter.AddListener<GameOverDto>(EventType.GameOverBRO, GameOverBRO);
        audio = GetComponent<AudioSource>();
        btnAgain = transform.Find("btnAgain").GetComponent<Button>();
        btnAgain.onClick.AddListener(OnAgainBtnClick);
        btnHome = transform.Find("btnHome").GetComponent<Button>();
        btnHome.onClick.AddListener(OnHomeBtnClick);
    }

    private void OnDestroy() {
        EventCenter.RemoveListener<GameOverDto>(EventType.GameOverBRO, GameOverBRO); // 重新加载当前场景
    }

    private void GameOverBRO(GameOverDto dto) {

        transform.DOScale(Vector3.one, 0.3f);

        win.txtName.text = dto.winDto.name;
        win.txtCoin.text = dto.winCount.ToString();

        lose1.txtName.text = dto.loseDtoList[0].name;
        lose1.txtCoin.text = (-dto.loseDtoList[0].stakesSum).ToString();

        lose2.txtName.text = dto.loseDtoList[1].name;
        lose2.txtCoin.text = (-dto.loseDtoList[1].stakesSum).ToString();

        if (dto.winDto.id == Models.GameModel.userDto.id) {
            audio.clip = winClip;
            audio.Play();
        }
        else {
            audio.clip = loseClip;
            audio.Play();
        }
    }

    /// <summary>
    /// 回到主菜单按钮点击事件
    /// </summary>
    private void OnHomeBtnClick() {
        SceneManager.LoadScene("02 - Main");
    }

    /// <summary>
    /// 再来一局按钮点击事件
    /// </summary>
    private void OnAgainBtnClick() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}
