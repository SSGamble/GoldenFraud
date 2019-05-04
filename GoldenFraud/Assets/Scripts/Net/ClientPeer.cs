using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class ClientPeer {

    private Socket clientSocket;
    private NetMsg msg;

    public ClientPeer() {
        try {
            msg = new NetMsg();
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
        catch (System.Exception e) {
            Debug.Log(e.Message);
        }
    }

    /// <summary>
    /// 连接服务器
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="port"></param>
    public void Connect(string ip, int port) {
        clientSocket.Connect(new IPEndPoint(IPAddress.Parse(ip), port));
        Debug.Log("连接服务器成功");
        StartReceive();
    }

    #region 接收数据

    /// <summary>
    /// 数据暂存区
    /// </summary>
    private byte[] receiveBuffer = new byte[1024];
    /// <summary>
    /// 数据缓存
    /// </summary>
    private List<byte> receiveCache = new List<byte>();
    /// <summary>
    /// 是否正在处理接收到的数据
    /// </summary>
    private bool isProcessingReceive = false;
    /// <summary>
    /// 存放消息队列
    /// </summary>
    public Queue<NetMsg> netMsgQueue = new Queue<NetMsg>();

    /// <summary>
    /// 开始接收数据
    /// </summary>
    private void StartReceive() {
        //Debug.Log(msg.opCode + "ClientPeer - StartReceive - 开始接收数据");
        if (clientSocket == null && clientSocket.Connected == false) {
            Debug.LogError("没有连接成功，无法接收消息");
            return;
        }
        clientSocket.BeginReceive(receiveBuffer, 0, 1024, SocketFlags.None, ReceiveCallback, clientSocket);
    }

    /// <summary>
    /// 开始接收完成后的回调
    /// </summary>
    /// <param name="ar"></param>
    private void ReceiveCallback(IAsyncResult ar) {
        //Debug.Log("开始接收完成后的回调");
        int length = clientSocket.EndReceive(ar);
        byte[] data = new byte[length];
        Buffer.BlockCopy(receiveBuffer, 0, data, 0, length);
        receiveCache.AddRange(data);
        if (isProcessingReceive == false) {
            ProcessReceive();
        }
        //Debug.Log("ClientPeer - ReceiveCallback - 开始接收完成后的回调" + isProcessingReceive + length);
        StartReceive(); // 伪递归，一直接收服务器发来的数据
    }

    /// <summary>
    /// 处理接收到的数据
    /// </summary>
    private void ProcessReceive() {
        //Debug.Log("ClientPeer - ProcessReceive - 处理接收的数据");
        isProcessingReceive = true;
        // 将接收到的数据转成 NetMsg 对象
        byte[] packet = EncodeTool.DecodePacket(ref receiveCache);
        if (packet == null) {
            isProcessingReceive = false;
            return;
        }
        //Debug.Log("packe - " + packet.Length);
        NetMsg msg = EncodeTool.DecodeMsg(packet);
        //Debug.Log("处理接收到的数据");
        // 加入消息队列
        netMsgQueue.Enqueue(msg);
        ProcessReceive(); // 递归
    }

    #endregion

    #region 发送数据
    /// <summary>
    /// 发送数据
    /// </summary>
    public void SendMsg(int opCode, int subCode, object value) {
        msg.Change(opCode, subCode, value);
        SendMsg(msg);
    }

    public void SendMsg(NetMsg msg) {
        try {
            byte[] data = EncodeTool.EncodeMsg(msg);
            byte[] packet = EncodeTool.EncodePacket(data);
            clientSocket.Send(packet);
        }
        catch (Exception e) {
            Debug.LogError(e.Message);
        }
    }
    #endregion
}
