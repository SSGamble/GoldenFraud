using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

/// <summary>
/// 游戏类型：单机/联网
/// </summary>
public enum GameType {
    Online,
    Stand
}

public class RoomChoosePanel : MonoBehaviour {

    private Button btnJoin1;
    private Button btnJoin2;
    private Button btnJoin3;
    private Button btnClose;
    private GameType gameType;

    private void Awake() {
        EventCenter.AddListener<GameType>(EventType.ShowRoomChoosePanel, ShowRoomChoosePanel);
        Init();
    }
    private void OnDestroy() {
        EventCenter.RemoveListener<GameType>(EventType.ShowRoomChoosePanel, ShowRoomChoosePanel);
    }

    private void Init() {
        btnJoin1 = transform.Find("Room1/btnJoin").GetComponent<Button>();
        btnJoin1.onClick.AddListener(delegate { EnterRoom(10, 100); });
        btnJoin2 = transform.Find("Room2/btnJoin").GetComponent<Button>();
        btnJoin2.onClick.AddListener(delegate { EnterRoom(20, 200); });
        btnJoin3 = transform.Find("Room3/btnJoin").GetComponent<Button>();
        btnJoin3.onClick.AddListener(OnJoin3ButtonClick);
        btnClose = transform.Find("btnClose").GetComponent<Button>();
        btnClose.onClick.AddListener(() => {
            transform.DOScale(Vector3.zero, 0.3f);
        });
    }

    private void ShowRoomChoosePanel(GameType type) {
        gameType = type;
        transform.DOScale(Vector3.one, 0.3f);
    }

    /// <summary>
    /// 进入房间
    /// </summary>
    /// <param name="bottomStakes"></param>
    /// <param name="topStakes"></param>
    private void EnterRoom(int bottomStakes, int topStakes) {

        // 将赌注信息存入游戏数据里
        Models.GameModel.BottomStakes = bottomStakes;
        Models.GameModel.TopStakes = topStakes;

        switch (gameType) {
            case GameType.Online:
                break;
            case GameType.Stand:
                break;
            default:
                break;
        }
    }

    private void OnJoin3ButtonClick() {
        EnterRoom(50,500);
    }
}
