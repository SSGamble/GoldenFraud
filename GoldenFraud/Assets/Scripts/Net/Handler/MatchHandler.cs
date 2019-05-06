using Protocol.Code;
using Protocol.Dto;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchHandler : BaseHandler {

    public override void OnReceive(int subCode, object value) {
        switch (subCode)
        {
            case MatchCode.Enter_SRES:
                EnterRoomSRES(value as MatchRoomDto);
                break;
            case MatchCode.Enter_BRO:
                Enter_BRO(value as UserDto);
                break;
            case MatchCode.Leave_BRO:
                Leave_BRO((int)value);
                break;
            case MatchCode.Ready_BRO:
                Ready_BRO((int)value);
                break;
            case MatchCode.UnReady_BRO:
                UnReady_BRO((int)value);
                break;
            case MatchCode.StartGame_BRO:
                StartGame_BRO();
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 开始游戏
    /// </summary>
    private void StartGame_BRO()
    {
        EventCenter.Broadcast(EventType.Hint, "开始发牌");
        EventCenter.Broadcast(EventType.StartGame);
    }

    /// <summary>
    /// 有玩家取消准备的广播
    /// </summary>
    /// <param name="value"></param>
    private void UnReady_BRO(int userId)
    {
        Models.GameModel.MatchRoomDto.UnReady(userId);
        // 刷新界面左右边玩家的 UI 显示
        EventCenter.Broadcast(EventType.RefreshUI);
    }

    /// <summary>
    /// 有玩家准备的广播
    /// </summary>
    /// <param name="userId"></param>
    private void Ready_BRO(int userId)
    {
        Models.GameModel.MatchRoomDto.Ready(userId);
        // 刷新界面左右边玩家的 UI 显示
        EventCenter.Broadcast(EventType.RefreshUI);
    }

    /// <summary>
    /// 有玩家离开房间的广播
    /// </summary>
    private void Leave_BRO(int userId)
    {
        Models.GameModel.MatchRoomDto.Leave(userId);
        ResetPos();
        // 刷新界面左右边玩家的 UI 显示
        EventCenter.Broadcast(EventType.RefreshUI);
    }

    /// <summary>
    /// 客户端请求进入房间服务器的响应
    /// </summary>
    private void EnterRoomSRES(MatchRoomDto matchRoomDto)
    {
        Models.GameModel.MatchRoomDto = matchRoomDto;
        ResetPos();
        // 刷新界面左右边玩家的 UI 显示
        EventCenter.Broadcast(EventType.RefreshUI);
    }

    /// <summary>
    /// 他人进入服务器的广播
    /// </summary>
    /// <param name="userDto"></param>
    private void Enter_BRO(UserDto dto)
    {
        Models.GameModel.MatchRoomDto.Enter(dto);
        ResetPos();
        // 刷新界面左右边玩家的 UI 显示
        EventCenter.Broadcast(EventType.RefreshUI);
        EventCenter.Broadcast(EventType.Hint, "玩家 " + dto.name + " 进入房间");
    }

    /// <summary>
    /// 给房间内的玩家排序
    /// </summary>
    private void ResetPos()
    {
        MatchRoomDto dto = Models.GameModel.MatchRoomDto;
        dto.ResetPosition(Models.GameModel.userDto.id); 
    }
}
