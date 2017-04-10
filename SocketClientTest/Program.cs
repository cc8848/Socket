using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketClientTest
{
    class Program
    {
        static SocketLibrary.Client client;
        static void Main(string[] args)
        {
            client = new SocketLibrary.Client("192.168.3.150", 8088);//此处输入自己的计算机IP地址，端口不能改变
            client.MessageReceived += _client_MessageReceived;
            client.MessageSent += client_MessageSent;
            client.StartClient();
            while (true)
            {
                System.Threading.Thread.Sleep(200);
                sendMsg();
            }
        }

        private static void client_MessageSent(object sender, SocketLibrary.SocketBase.MessageEventArgs e)
        {
            Console.WriteLine(e.Connecction.ConnectionName + "发送成功");
        }
        private static void _client_MessageReceived(object sender, SocketLibrary.SocketBase.MessageEventArgs e)
        {
            string msg = e.Message.MessageBody;

            Console.WriteLine(e.Connecction.ConnectionName + msg + ":发送成功");
        }
        private static void sendMsg()
        {
            SocketLibrary.Connection connection;
            client.Connections.TryGetValue(client.ClientName, out connection);
            if (connection != null)
            {
                SocketLibrary.Message message = new SocketLibrary.Message(SocketLibrary.Message.CommandType.SendMessage, "消息体");
                connection.messageQueue.Enqueue(message);
            }
            else
            {
                Console.WriteLine("发送失败！");
            }
        }
    }
}
