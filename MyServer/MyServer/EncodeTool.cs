using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace MyServer {

    /// <summary>
    /// 编码工具
    ///     解决 「TCP 的粘包和拆包」 问题
    /// </summary>
    public class EncodeTool {

        /// <summary>
        /// 构造包，包头和包尾
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] EncodePacket(byte[] data) {
            using (MemoryStream ms = new MemoryStream()) { // 内存流对象
                using (BinaryWriter bw = new BinaryWriter(ms)) { // 二进制写

                    // 写入包头（数据的长度）
                    bw.Write(data.Length);
                    // 写入包尾（数据）
                    bw.Write(data);

                    // 将写入的数据复制给 packet
                    byte[] packet = new byte[ms.Length];
                    Buffer.BlockCopy(ms.GetBuffer(), 0, packet, 0, (int)ms.Length);

                    return packet;
                }
            }
        }

        /// <summary>
        /// 解析包，从缓冲区里取出一个完整的包
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public static byte[] DecodePacket(ref List<byte> cache) {
            if (cache.Count < 4) { // 还没有一个整型长，不足以写入数据的长度，所以表示里面没有数据
                return null;
            }
            using (MemoryStream ms = new MemoryStream(cache.ToArray())) {
                using (BinaryReader br = new BinaryReader(ms)) {
                    int length = br.ReadInt32(); // 包的长度
                    int remainLength = (int)(ms.Length - ms.Position); // 剩余的长度
                    if (length > remainLength) { // 剩余的长度已经构不成包了
                        return null;
                    }
                    byte[] data = br.ReadBytes(length);
                    // 更新缓存
                    cache.Clear();
                    cache.AddRange(br.ReadBytes(remainLength));
                    return data;
                }
            }
        }

        /// <summary>
        /// 把 NetMsg类 转换为字节数组，发送出去
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static byte[] EncodeMsg(NetMsg msg) {
            using (MemoryStream ms = new MemoryStream()) {
                using (BinaryWriter bw = new BinaryWriter(ms)) {
                    bw.Write(msg.opCode);
                    bw.Write(msg.subCode);
                    if (msg.value != null) {
                        bw.Write(EncodeObj(msg.value));
                    }
                    byte[] data = new byte[ms.Length];
                    Buffer.BlockCopy(ms.GetBuffer(), 0, data, 0, (int)ms.Length);

                    return data;
                }
            }
        }

        /// <summary>
        /// 将字节数组转换成 NetMsg 网络消息类
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static NetMsg DecodeMsg(byte[] data) {
            using (MemoryStream ms = new MemoryStream(data)) {
                using (BinaryReader br = new BinaryReader(ms)) {
                    NetMsg msg = new NetMsg();
                    msg.opCode = br.ReadInt32();
                    msg.subCode = br.ReadInt32();
                    if (ms.Length - ms.Position > 0) {
                        object obj = DecodeObj(br.ReadBytes((int)(ms.Length - ms.Position)));
                        msg.value = obj;
                    }
                    return msg;
                }
            }
        }

        /// <summary>
        /// 序列化，将对象转为字节数组
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static byte[] EncodeObj(object obj) {
            using (MemoryStream ms = new MemoryStream()) {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, obj); // 序列化，将 obj 存到 ms 里面

                byte[] data = new byte[ms.Length];
                Buffer.BlockCopy(ms.GetBuffer(), 0, data, 0, (int)ms.Length);

                return data;
            }
        }

        /// <summary>
        /// 反序列化，将字节数组转换成对象
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static object DecodeObj(byte[] data) {
            using (MemoryStream ms = new MemoryStream(data)) {
                BinaryFormatter bf = new BinaryFormatter();
                return bf.Deserialize(ms);
            }
        }
    }
}