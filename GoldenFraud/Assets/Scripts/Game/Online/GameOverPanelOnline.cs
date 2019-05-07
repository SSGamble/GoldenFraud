using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Protocol.Code;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverPanelOnline : MonoBehaviour {

    [System.Serializable]
    public class Player {
        public Text txtName;
        public Text txtCoin;
        public Image imgWin;
        public Image imgLose;
    }

    public Player leftPlayer;
    public Player rightPlayer;
    public Player selfPlayer;

    private Button btnAgain;
    private Button btnHome;

    private AudioSource audio;
    public AudioClip winClip;
    public AudioClip loseClip;

    private void Awake() {
        EventCenter.AddListener<int, int, int>(EventType.GameOver, GameOver);
        audio = GetComponent<AudioSource>();
        btnAgain = transform.Find("btnAgain").GetComponent<Button>();
        btnAgain.onClick.AddListener(OnAgainBtnClick);
        btnHome = transform.Find("btnHome").GetComponent<Button>();
        btnHome.onClick.AddListener(OnHomeBtnClick);
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

    private void OnDestroy() {
        EventCenter.RemoveListener<int, int, int>(EventType.GameOver, GameOver); // 重新加载当前场景
    }

    private void GameOver(int leftCoin, int selfCoin, int rightCoin) {
        transform.DOScale(Vector3.one, 0.3f);

        leftPlayer.imgLose.gameObject.SetActive(false);
        leftPlayer.imgWin.gameObject.SetActive(false);
        selfPlayer.imgLose.gameObject.SetActive(false);
        selfPlayer.imgWin.gameObject.SetActive(false);
        rightPlayer.imgLose.gameObject.SetActive(false);
        rightPlayer.imgWin.gameObject.SetActive(false);

        // 左边输
        if (leftCoin < 0) {
            leftPlayer.imgLose.gameObject.SetActive(true);
            leftPlayer.txtCoin.text = leftCoin.ToString();
        }
        // 左边赢
        else {
            leftPlayer.imgWin.gameObject.SetActive(true);
            leftPlayer.txtCoin.text = (Mathf.Abs(selfCoin + rightCoin) + leftCoin).ToString();
        }
        // 自身输
        if (selfCoin < 0) {
            audio.clip = loseClip;
            audio.Play();
            if (NetMsgCenter.Instance != null) {
                NetMsgCenter.Instance.SendMsg(OpCode.Account, AccountCode.UpdateCoin_CREQ, -selfCoin);
            }
            selfPlayer.imgLose.gameObject.SetActive(true);
            selfPlayer.txtCoin.text = selfCoin.ToString();
        }
        // 自身赢
        else {
            audio.clip = winClip;
            audio.Play();
            int winCoin = Mathf.Abs(leftCoin + rightCoin) + selfCoin;
            if (NetMsgCenter.Instance != null) {
                NetMsgCenter.Instance.SendMsg(OpCode.Account, AccountCode.UpdateCoin_CREQ, winCoin);
            }
            selfPlayer.imgWin.gameObject.SetActive(true);
            selfPlayer.txtCoin.text = winCoin.ToString();
        }
        // 右边输
        if (rightCoin < 0) {
            rightPlayer.imgLose.gameObject.SetActive(true);
            rightPlayer.txtCoin.text = rightCoin.ToString();
        }
        // 右边赢
        else {
            rightPlayer.imgWin.gameObject.SetActive(true);
            rightPlayer.txtCoin.text = (Mathf.Abs(leftCoin + selfCoin) + rightCoin).ToString();
        }
    }
}
