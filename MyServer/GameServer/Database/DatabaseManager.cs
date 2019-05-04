using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyServer;
using MySql.Data.MySqlClient;
using Protocol.Dto;

namespace GameServer.Database {
    public class DatabaseManager {

        private static MySqlConnection sqlConnection;
        private static Dictionary<int, ClientPeer> idClientDic;
        private static RankListDto rankListDto;

        /// <summary>
        /// 连接数据库
        /// </summary>
        public static void StartConnection() {
            rankListDto = new RankListDto();
            idClientDic = new Dictionary<int, ClientPeer>();
            string conStr = "database=golden_fraud;data source=127.0.0.1;port=3306;user=root;pwd=root";
            sqlConnection = new MySqlConnection(conStr);
            sqlConnection.Open();
        }

        /// <summary>
        /// 是否存在该用户
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public static bool IsExistUserName(string name) {
            MySqlCommand cmd = new MySqlCommand("select name from user where name=@name", sqlConnection);
            cmd.Parameters.AddWithValue("name", name);
            MySqlDataReader reader = cmd.ExecuteReader();
            bool result = reader.HasRows;
            reader.Close();
            return result;
        }

        /// <summary>
        /// 添加用户
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pwd"></param>
        public static void AddUser(string name, string pwd) {
            MySqlCommand cmd = new MySqlCommand("insert into user set name=@name,pwd=@pwd,online=0,icon_name=@iconName", sqlConnection);
            cmd.Parameters.AddWithValue("name", name);
            cmd.Parameters.AddWithValue("pwd", pwd);
            // 随机一个头像
            Random ran = new Random();
            int index = ran.Next(0, 19);
            cmd.Parameters.AddWithValue("iconName", "headIcon_" + index.ToString());
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// 用户名和密码是否匹配
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pwd"></param>
        public static bool IsMatch(string name, string pwd) {
            MySqlCommand cmd = new MySqlCommand("select * from user where name=@name", sqlConnection);
            cmd.Parameters.AddWithValue("name", name);
            MySqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows) {
                reader.Read();
                bool result = reader.GetString("pwd") == pwd;
                reader.Close();
                return result;
            }
            reader.Close();
            return false;
        }

        /// <summary>
        /// 判断用户是否在线
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool IsOnline(string name) {
            MySqlCommand cmd = new MySqlCommand("select online from user where name=@name", sqlConnection);
            cmd.Parameters.AddWithValue("name", name);
            MySqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows) {
                reader.Read();
                bool result = reader.GetBoolean("online");
                reader.Close();
                return result;
            }
            reader.Close();
            return false;
        }

        /// <summary>
        /// 登录上线
        /// </summary>
        /// <param name="client"></param>
        /// <param name="name"></param>
        public static void Login(ClientPeer client, string name) {

            // 更新 online 字段
            MySqlCommand cmd = new MySqlCommand("update user set online = true where name = @name", sqlConnection);
            cmd.Parameters.AddWithValue("name", name);
            cmd.ExecuteNonQuery();

            MySqlCommand cmd1 = new MySqlCommand("select * from user where name=@name", sqlConnection);
            cmd1.Parameters.AddWithValue("name", name);
            MySqlDataReader reader = cmd1.ExecuteReader();
            if (reader.HasRows) {
                reader.Read();
                int id = reader.GetInt32("id");
                client.Id = id;
                client.UserName = name;
                if (idClientDic.ContainsKey(id) == false) {
                    idClientDic.Add(id, client); // 将 id 和 对应的 client 加入字典
                }
                reader.Close();
            }
            reader.Close();
        }

        /// <summary>
        /// 用户下线
        /// </summary>
        /// <param name="client"></param>
        /// <param name="name"></param>
        public static void OffLine(ClientPeer client) {
            // 从字典中移除
            if (idClientDic.ContainsKey(client.Id)) {
                idClientDic.Remove(client.Id);
            }
            // 更新 online 字段
            MySqlCommand cmd = new MySqlCommand("update user set online = false where id = @id", sqlConnection);
            cmd.Parameters.AddWithValue("id", client.Id);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// 使用用户id获取客户端连接对象
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static ClientPeer GetClientPeerByUserId(int id) {
            if (idClientDic.ContainsKey(id)) {
                return idClientDic[id];
            }
            return null;
        }

        /// <summary>
        /// 构造用户信息传输模型
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static UserDto CreateUserDto(int id) {
            MySqlCommand cmd = new MySqlCommand("select * from user where id=@id", sqlConnection);
            cmd.Parameters.AddWithValue("id", id);
            MySqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows) {
                reader.Read();
                UserDto dto = new UserDto(id, reader.GetString("name"), reader.GetString("icon_name"), reader.GetInt32("coin"));
                reader.Close();
                return dto;
            }
            reader.Close();
            return null;
        }

        /// <summary>
        /// 获取排行榜信息
        /// </summary>
        /// <returns></returns>
        public static RankListDto GetRankListDto() {
            MySqlCommand cmd = new MySqlCommand("select name,coin from user order by coin desc", sqlConnection);
            MySqlDataReader reader = cmd.ExecuteReader();
            rankListDto.Clear(); // 先清空排行榜信息
            if (reader.HasRows) {
                while (reader.Read()) {
                    RankItemDto dto = new RankItemDto(reader.GetString("name"), reader.GetInt32("coin"));
                    rankListDto.Add(dto);
                }
                reader.Close();
                return rankListDto;
            }
            reader.Close();
            return null;
        }

        /// <summary>
        /// 更新金币数量，返回更新后的数量
        /// </summary>
        /// <param name="id"></param>
        /// <param name="coin"></param>
        /// <returns></returns>
        public static int UpdateCoin(int id, int coin) {
            MySqlCommand cmd = new MySqlCommand("select coin from user where id = @id", sqlConnection);
            cmd.Parameters.AddWithValue("id", id);
            MySqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows) {
                reader.Read();
                int remainCoin = reader.GetInt32("coin");
                reader.Close();

                MySqlCommand cmd1 = new MySqlCommand("update user set coin = @coin where id = @id", sqlConnection);
                cmd1.Parameters.AddWithValue("id", id);
                cmd1.Parameters.AddWithValue("coin", remainCoin + coin);
                cmd1.ExecuteNonQuery();

                return remainCoin + coin;
            }
            reader.Close();
            return 0;
        }
    }
}
