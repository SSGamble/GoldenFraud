using System.Collections;
using System.Collections.Generic;
using Protocol.Dto;
using UnityEngine;

/// <summary>
/// 游戏数据
/// </summary>
public class GameModel {

    /// <summary>
    /// 用户信息
    /// </summary>
	public UserDto userDto { get; set; }
    /// <summary>
    /// 底注
    /// </summary>
    public int BottomStakes { get; set; }
    /// <summary>
    /// 顶注
    /// </summary>
    public int TopStakes { get; set; }
    /// <summary>
    /// 房间类型
    /// </summary>
    public RoomType RoomType { get; set; }

}
