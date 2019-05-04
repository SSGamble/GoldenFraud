using System;
using System.Collections;
using System.Collections.Generic;
using Protocol.Code;
using Protocol.Dto;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AccountHandler : BaseHandler {

    public override void OnReceive(int subCode, object value) {
        switch (subCode) {
            case AccountCode.Register_SRES:
                Register_SRES((int)value);
                break;
            case AccountCode.Login_SRES:
                Login_SRES((int)value);
                break;
            case AccountCode.GetUserInfo_SRES:
                Models.GameModel.userDto = (UserDto)value;
                SceneManager.LoadScene("02 - Main"); // 跳转主场景
                break;
            case AccountCode.GetRank_SRES:
                EventCenter.Broadcast(EventType.SendRankListDto, value as RankListDto); // 让排行榜面板自己解析数据
                break;
            case AccountCode.UpdateCoin_SRES:
                Models.GameModel.userDto.coin = (int)value; // 将金币数量更新到数据模型里面
                EventCenter.Broadcast(EventType.UpdateCoin, (int)value); // 广播更新金币
                break;
            default:
                break;
        }
    }


    /// <summary>
    /// 登录服务器的响应
    /// </summary>
    /// <param name="value"></param>
    private void Login_SRES(int value) {
        switch (value) {
            case -1:
                EventCenter.Broadcast(EventType.Hint, "用户名不存在");
                break;
            case -2:
                EventCenter.Broadcast(EventType.Hint, "密码错误");
                break;
            case -3:
                EventCenter.Broadcast(EventType.Hint, "用户在线");
                break;
            case 1:
                NetMsgCenter.Instance.SendMsg(OpCode.Account, AccountCode.GetUserInfo_CREQ, null);
                EventCenter.Broadcast(EventType.Hint, "登录成功");
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 注册服务器的响应
    /// </summary>
    /// <param name="value"></param>
    private void Register_SRES(int value) {
        switch (value) {
            case -1:
                EventCenter.Broadcast(EventType.Hint, "用户名已被注册");
                return;
            case 0:
                EventCenter.Broadcast(EventType.Hint, "注册成功");
                break;
            default:
                break;
        }
    }
}
