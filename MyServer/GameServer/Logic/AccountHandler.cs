using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Protocol.Code;
using Protocol.Dto;
using GameServer.Database;
using MyServer;

namespace GameServer.Logic {

    public class AccountHandler : IHandler {

        /// <summary>
        /// 客户端断开连接
        /// </summary>
        public void Disconnect(ClientPeer client) {
            DatabaseManager.OffLine(client); // 用户下线
        }

        /// <summary>
        /// 接收客户端的请求
        /// </summary>
        public void Receive(ClientPeer client, int subCode, object value) {
            switch (subCode) {
                case AccountCode.Register_CREQ: // 客户端注册请求
                    Register(client, value as AccountDto);
                    break;
                case AccountCode.Login_CREQ: // 登录请求
                    Login(client, value as AccountDto);
                    break;
                case AccountCode.GetUserInfo_CREQ: // 获取用户信息请求
                    GetUserInfo(client);
                    break;
                case AccountCode.GetRank_CREQ: // 获取排行榜信息请求
                    GetRank(client);
                    break;
                case AccountCode.UpdateCoin_CREQ: // 更新金币数量请求，充值
                    UpdateCoin(client,(int)value);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 客户端发来的更新金币数量请求
        /// </summary>
        /// <param name="client"></param>
        /// <param name="coin"></param>
        private void UpdateCoin(ClientPeer client, int coin) {
            SingleExecute.Instance.Exeecute(() => {
                int totalCoin = DatabaseManager.UpdateCoin(client.Id,coin);
                client.SendMsg(OpCode.Account, AccountCode.UpdateCoin_SRES, totalCoin);
            });
        }

        /// <summary>
        /// 客户端获取排行榜信息的请求
        /// </summary>
        /// <param name="client"></param>
        private void GetRank(ClientPeer client) {
            SingleExecute.Instance.Exeecute(() => {
                RankListDto dto = DatabaseManager.GetRankListDto();
                client.SendMsg(OpCode.Account, AccountCode.GetRank_SRES, dto);
            });
        }

        /// <summary>
        /// 客户端获取用户信息的请求
        /// </summary>
        /// <param name="client"></param>
        private void GetUserInfo(ClientPeer client) {
            SingleExecute.Instance.Exeecute(() => {
                UserDto dto = DatabaseManager.CreateUserDto(client.Id);
                client.SendMsg(OpCode.Account, AccountCode.GetUserInfo_SRES, dto);
            });
        }

        /// <summary>
        /// 客户端注册的处理
        /// </summary>
        /// <param name="dto"></param>
        private void Register(ClientPeer client, AccountDto dto) {
            // 单线程执行，防止多个线程同时访问，数据出错
            SingleExecute.Instance.Exeecute(() => {
                // 用户名已被注册
                if (DatabaseManager.IsExistUserName(dto.userName)) {
                    client.SendMsg(OpCode.Account, AccountCode.Register_SRES, -1); // 发送一个整型，客户端接收后自己进行判断，比直接发送一个"用户名已被注册"的字符串节约性能
                    return;
                }
                // 添加用户
                DatabaseManager.AddUser(dto.userName, dto.password);
                // 给客户端返回一个消息，0 - 代表注册成功
                client.SendMsg(OpCode.Account, AccountCode.Register_SRES, 0);
            });
        }

        /// <summary>
        /// 客户端登录的处理
        /// </summary>
        /// <param name="client"></param>
        /// <param name="dto"></param>
        private void Login(ClientPeer client, AccountDto dto) {

            // 单线程执行，防止多个线程同时访问，数据出错
            SingleExecute.Instance.Exeecute(() => {
                // 用户名不存在
                if (DatabaseManager.IsExistUserName(dto.userName) == false) {
                    client.SendMsg(OpCode.Account, AccountCode.Login_SRES, -1); // 向客户端发送消息
                    return;
                }
                // 密码错误
                if (DatabaseManager.IsMatch(dto.userName,dto.password) == false) {
                    client.SendMsg(OpCode.Account, AccountCode.Login_SRES, -2);
                    return;
                }
                // 用户在线
                if (DatabaseManager.IsOnline(dto.userName)) {
                    client.SendMsg(OpCode.Account, AccountCode.Login_SRES, -3);
                    return;
                }

                // 验证都通过了，登录成功
                DatabaseManager.Login(client, dto.userName);
                client.SendMsg(OpCode.Account, AccountCode.Login_SRES, 1);
            });
        }
    }
}
