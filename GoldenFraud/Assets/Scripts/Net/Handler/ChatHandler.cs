using System.Collections;
using System.Collections.Generic;
using Protocol.Code;
using Protocol.Dto.Fight;
using UnityEngine;

public class ChatHandler : BaseHandler {

    public override void OnReceive(int subCode, object value) {
        switch (subCode) {
            case ChatCode.BRO:
                EventCenter.Broadcast(EventType.ChatBRO, (ChatDto)value);
                break;
            default:
                break;
        }
    }
}
