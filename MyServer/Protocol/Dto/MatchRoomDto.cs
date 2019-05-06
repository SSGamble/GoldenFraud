using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Protocol.Dto {
    /// <summary>
    /// 匹配房间 传输模型
    /// </summary>
    [Serializable]
    public class MatchRoomDto {

        /// <summary>
        /// 用户ID 和 用户Dto 字典
        /// </summary>
        public Dictionary<int, UserDto> userIdUserDto { get; set; }
        /// <summary>
        /// 准备的用户id列表
        /// </summary>
        public List<int> readyUserList { get; set; }
        /// <summary>
        /// 进入房间顺序的用户id列表
        /// </summary>
        public List<int> enterOrderUserIdList { get; set; }
        public int LeftPlayerId { get; private set; }
        public int RightPlayerId { get; private set; }

        public MatchRoomDto() {
            userIdUserDto = new Dictionary<int, UserDto>();
            readyUserList = new List<int>();
            enterOrderUserIdList = new List<int>();
        }

        /// <summary>
        /// 进入房间
        /// </summary>
        /// <param name="dto"></param>
        public void Enter(UserDto dto) {
            userIdUserDto.Add(dto.id, dto);
            enterOrderUserIdList.Add(dto.id);
        }

        /// <summary>
        /// 离开房间
        /// </summary>
        /// <param name="userId"></param>
        public void Leave(int userId) {
            userIdUserDto.Remove(userId);
            readyUserList.Remove(userId);
            enterOrderUserIdList.Remove(userId);
        }

        /// <summary>
        /// 准备
        /// </summary>
        /// <param name="userId"></param>
        public void Ready(int userId) {
            readyUserList.Add(userId);
        }

        /// <summary>
        /// 取消准备
        /// </summary>
        /// <param name="userId"></param>
        public void UnReady(int userId) {
            readyUserList.Remove(userId);
        }

        /// <summary>
        /// 重置位置，给 3 个玩家排序
        /// </summary>
        /// <param name="myUserId"></param>
        public void ResetPosition(int myUserId) {
            RightPlayerId = -1;
            LeftPlayerId = -1;
            // 进来一个玩家
            if (enterOrderUserIdList.Count == 1) {
                return;
            }
            // 进来两个玩家
            if (enterOrderUserIdList.Count == 2) {

                // 自己的右边有一个玩家 x a
                if (enterOrderUserIdList[0] == myUserId) {
                    RightPlayerId = enterOrderUserIdList[1];
                }
                // 自己的左边有一个玩家 a x
                if (enterOrderUserIdList[1] == myUserId) {
                    LeftPlayerId = enterOrderUserIdList[0];
                }
            }
            // 进来三个玩家
            if (enterOrderUserIdList.Count == 3) {

                // x a b
                if (enterOrderUserIdList[0] == myUserId) {
                    RightPlayerId = enterOrderUserIdList[1];
                    LeftPlayerId = enterOrderUserIdList[2];
                }
                // a x b
                if (enterOrderUserIdList[1] == myUserId) {
                    RightPlayerId = enterOrderUserIdList[2];
                    LeftPlayerId = enterOrderUserIdList[0];
                }
                // a b x
                if (enterOrderUserIdList[2] == myUserId) {
                    RightPlayerId = enterOrderUserIdList[0];
                    LeftPlayerId = enterOrderUserIdList[1];
                }
            }
        }
    }
}
