using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using SocketLibrary;

namespace SocketClientTest
{
    class Program
    {
        static SocketLibrary.Client client;
        static void Main(string[] args)
        {
            client = new SocketLibrary.Client("127.0.0.1", 8878);//此处输入自己的计算机IP地址，端口不能改变
            client.MessageReceived += _client_MessageReceived;
            client.MessageSent += client_MessageSent;
            client.Connected+= ClientOnConnected;
            client.ConnectionClose += ClientOnConnectionClose;
            client.StartClient();
            while (true)
            {
                System.Threading.Thread.Sleep(200);
                sendMsg();
            }
        }

        private static void ClientOnConnectionClose(object sender, SocketBase.ConCloseMessagesEventArgs e)
        {
           Console.WriteLine("ClientOnConnectionClose  "+e.ConnectionName);
        }

        private static void ClientOnConnected(object sender, Connection e)
        {
            Console.WriteLine("ClientOnConnected  " + e.ConnectionName);
        }

        private static Task client_MessageSent(object sender, SocketLibrary.SocketBase.MessageEventArgs e)
        {
            return Task.CompletedTask;
            // Console.WriteLine(e.Connecction.ConnectionName + "");
        }
        private static  Task _client_MessageReceived(object sender, SocketLibrary.SocketBase.MessageEventArgs e)
        {
            string msg = e.Message.MessageBody;

            Console.WriteLine(e.Connecction.ConnectionName + msg + ":Received");
            return  Task.CompletedTask;
        }

        private static int conunt = 1;
        private static void sendMsg()
        {
            conunt++;
            Console.WriteLine(conunt);
            client.Connections.TryGetValue(client.ClientName, out var connection);
            if (connection != null)
            {
                SocketLibrary.Message message = new SocketLibrary.Message(SocketLibrary.Message.CommandType.SendMessage, "消消息体消息体消息体消息体消息体消息体消息体消息体消息体消息体消息体消息体消息体消息体消息体消息体消息体消息体消息体消息体消息体消息体消息体消息体消息体消息体消息体消息体消息体消息体消息体消息体消息体消息体消息体消息体消息体消息体消息体消息体消息体消息体消息体消息体消息体消息体消息体消息体消息体消息体消息体消息体息体"+ conunt);
                connection.messageQueue.Enqueue(message);
            }
            else
            {
                Console.WriteLine("发送失败！");
            }
        }
    }
}
