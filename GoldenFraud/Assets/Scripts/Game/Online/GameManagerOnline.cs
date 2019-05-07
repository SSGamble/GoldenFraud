using System.Collections;
using System.Collections.Generic;
using Protocol.Code;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManagerOnline : MonoBehaviour {

    private Text txtBottomStakes;
    private Text txtTopStakes;
    private Button btnBack;
    private SelfManagerOnlint selfManager;
    private LeftManagerOnline leftManager;
    private RightManagerOnline rightManager;

    public bool LeftIsLeave { get { return leftManager.isRun; } }
    public bool LeftIsGiveUp { get { return leftManager.isGiveUp; } }

    public bool RightIsLeave { get { return rightManager.isRun; } }
    public bool RightIsGiveUp { get { return rightManager.isGiveUp; } }

    private void Awake() {
        if (NetMsgCenter.Instance!=null)
        {
            NetMsgCenter.Instance.SendMsg(OpCode.Match, MatchCode.Enter_CREQ, (int)Models.GameModel.RoomType); // 把房间类型发过去
        }
        Init();
    }

    private void Init() {
        selfManager = GetComponentInChildren<SelfManagerOnlint>();
        leftManager = GetComponentInChildren<LeftManagerOnline>();
        rightManager = GetComponentInChildren<RightManagerOnline>();

        txtBottomStakes = transform.Find("Main/txtBottomStakes").GetComponent<Text>();
        txtTopStakes = transform.Find("Main/txtTopStakes").GetComponent<Text>();
        btnBack = transform.Find("Main/btnBack").GetComponent<Button>();
        btnBack.onClick.AddListener(() => {
            SceneManager.LoadScene("02 - Main");
            // 向服务器发送离开房间的请求
            NetMsgCenter.Instance.SendMsg(OpCode.Match, MatchCode.Leave_CREQ, (int)Models.GameModel.RoomType);
            NetMsgCenter.Instance.SendMsg(OpCode.Fight, FightCode.Leave_CREQ,null);
        });

        txtBottomStakes.text = Models.GameModel.BottomStakes.ToString();
        txtTopStakes.text = Models.GameModel.TopStakes.ToString();
    }
}
