using System.Collections;
using System.Collections.Generic;
using Protocol.Code;
using UnityEngine;
using UnityEngine.UI;

public class GameManagerOnline : MonoBehaviour {

    private Text txtBottomStakes;
    private Text txtTopStakes;
    private Button btnBack;

    private void Awake() {
        NetMsgCenter.Instance.SendMsg(OpCode.Match, MatchCode.Enter_CREQ, (int)Models.GameModel.RoomType); // 把房间类型发过去
        Init();
    }

    private void Init() {

        txtBottomStakes = transform.Find("Main/txtBottomStakes").GetComponent<Text>();
        txtTopStakes = transform.Find("Main/txtTopStakes").GetComponent<Text>();
        btnBack = transform.Find("Main/btnBack").GetComponent<Button>();
        btnBack.onClick.AddListener(() => {

        });

        txtBottomStakes.text = Models.GameModel.BottomStakes.ToString();
        txtTopStakes.text = Models.GameModel.TopStakes.ToString();
    }
}
