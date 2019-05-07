using System.Collections;
using System.Collections.Generic;
using Protocol.Code;
using Protocol.Dto.Fight;
using UnityEngine;
using UnityEngine.UI;

public class ChatPanelOnline : MonoBehaviour {

    public GameObject goChatItemPre;

    private Transform parent;
    private InputField iptMsg;
    private Button btnSend;

    private void Awake() {
        EventCenter.AddListener<ChatDto>(EventType.ChatBRO, ChatBRO);
        parent = transform.Find("Chat/ScrollRect/Parent");
        iptMsg = transform.Find("InputField").GetComponent<InputField>();
        btnSend = transform.Find("Button").GetComponent<Button>();
        btnSend.onClick.AddListener(OnSendButtonClick);
    }
    private void OnDestroy() {
        EventCenter.RemoveListener<ChatDto>(EventType.ChatBRO, ChatBRO);
    }

    /// <summary>
    /// 发送按钮点击事件
    /// </summary>
    private void OnSendButtonClick() {
        if (iptMsg.text == null || iptMsg.text == "") {
            return;
        }
        NetMsgCenter.Instance.SendMsg(OpCode.Chat, ChatCode.CREQ, iptMsg.text);
        iptMsg.text = "";
    }

    private void ChatBRO(ChatDto dto) {
        GameObject go = Instantiate(goChatItemPre, parent);
        if (dto.userId == Models.GameModel.userDto.id) { //如果是自己发的
            go.GetComponent<Text>().text = "我：" + dto.msg;
        }
        else {
            go.GetComponent<Text>().text = dto.userName + "：" + dto.msg;
        }
    }
}
