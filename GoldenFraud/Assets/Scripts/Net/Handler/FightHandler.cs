using System;
using System.Collections;
using System.Collections.Generic;
using Protocol.Code;
using Protocol.Constant;
using Protocol.Dto;
using Protocol.Dto.Fight;
using UnityEngine;

public class FightHandler : BaseHandler {

    public override void OnReceive(int subCode, object value) {
        switch (subCode) {
            case FightCode.StartFight_BRO:
                StartFight_BRO(value as List<PlayerDto>);
                break;
            case FightCode.Leave_BRO:
                EventCenter.Broadcast(EventType.LeaveFightRoom, (int)value);
                break;
            case FightCode.StartStakes_BRO:
                EventCenter.Broadcast(EventType.StartStakes, (int)value);
                break;
            case FightCode.LookCard_BRO:
                EventCenter.Broadcast(EventType.LookCardBRO, (int)value);
                break;
            case FightCode.PutStakes_BRO:
                EventCenter.Broadcast(EventType.PutStakesBRO, (StakesDto)value);
                break;
            case FightCode.GiveUpCard_BRO:
                EventCenter.Broadcast(EventType.GiveUpCardBRO, (int)value);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 开始战斗的服务器广播
    /// </summary>
    /// <param name="list"></param>
    private void StartFight_BRO(List<PlayerDto> playerList) {
        foreach (var player in playerList) {
            // 左边玩家
            if (player.id == Models.GameModel.MatchRoomDto.LeftPlayerId) {
                if (player.identity == Identity.Banker) {
                    EventCenter.Broadcast(EventType.LeftBanker);
                }
                EventCenter.Broadcast(EventType.LeftDealCard, player);
            }
            // 右边玩家
            if (player.id == Models.GameModel.MatchRoomDto.RightPlayerId) {
                if (player.identity == Identity.Banker) {
                    EventCenter.Broadcast(EventType.RightBanker);
                }
                EventCenter.Broadcast(EventType.RightDealCard, player);
            }
            // 自身玩家
            if (player.id == Models.GameModel.userDto.id) {
                if (player.identity == Identity.Banker) {
                    EventCenter.Broadcast(EventType.SelfBanker);
                }
                EventCenter.Broadcast(EventType.SelfDealCard, player);
            }
        }
    }
}
