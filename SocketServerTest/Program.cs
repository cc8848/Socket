﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SocketServerTest
{

    class Program
    {
        static int i = 0;
        static SocketLibrary.Server _server;
        static void Main(string[] args)
        {
            _server = new SocketLibrary.Server(IPAddress.Any, 8878);
            _server.MessageReceived += _server_MessageReceived;
            _server.Connected += _server_Connected;
            _server.ConnectionClose += _server_ConnectionClose;
            _server.MessageSent += _server_MessageSent;
            _server.StartServer();
            while (true)
            {
                System.Threading.Thread.Sleep(200);
            }
        }

        static Task _server_MessageSent(object sender, SocketLibrary.SocketBase.MessageEventArgs e)
        {
            Console.WriteLine(e.Connecction.ConnectionName + "服务端发送成功");
            return Task.CompletedTask;
        }
        private static async Task _server_ConnectionClose(object sender, SocketLibrary.SocketBase.ConCloseMessagesEventArgs e)
        {
            Console.WriteLine(e.ConnectionName + "连接关闭");
            
        }
        private static void _server_Connected(object sender, SocketLibrary.Connection e)
        {
            Console.WriteLine(e.ConnectionName + "连接成功");
        }
        private static async Task _server_MessageReceived(object sender, SocketLibrary.SocketBase.MessageEventArgs e)
        {
            var format = e.Message.MessageBody + $"\t Server FileTime: {DateTime.Now.ToFileTime()}";
            Console.WriteLine(format);
            SendMsg(format);
        }

        private static void SendMsg(string ss)
        {
            i += 1;
            // SocketLibrary.Connection connection = null;
            foreach (var keyValue in _server.Connections)
            {
                // if ("192.168.3.150".Equals(keyValue.Value.NickName))
                // {
                //     connection = keyValue.Value;
                // }
                SocketLibrary.Message message = new SocketLibrary.Message(SocketLibrary.Message.CommandType.SendMessage, i + ss);
                keyValue.Value .messageQueue.Enqueue(message);
            }
            // if (connection != null)
            // {
            //     SocketLibrary.Message message = new SocketLibrary.Message(SocketLibrary.Message.CommandType.SendMessage, i + "服务端发送消息体");
            //     connection.messageQueue.Enqueue(message);
            // }
            // else
            // {
            //     Console.WriteLine("发送失败！");
            // }
        }

    }
}
