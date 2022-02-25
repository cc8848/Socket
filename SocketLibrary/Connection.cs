using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SocketLibrary
{
    /// <summary>
    /// Connection 的摘要说明。
    /// </summary>
    public class Connection
    {
        #region 属性

        /// <summary>
        ///  提供用于网络访问的基础数据流
        /// </summary>
        public NetworkStream NetworkStream { get; private set; }

        /// <summary>
        /// 连接的Key
        /// </summary>
        public string ConnectionName { get; private set; }

        /// <summary>
        /// 连接别名
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// 此链接的消息队列
        /// </summary>
        public ConcurrentQueue<Message> messageQueue { get; protected set; }

        /// <summary>
        /// TcpClient
        /// </summary>
        public TcpClient TcpClient { get; private set; }
        /// <summary>
        /// 最后一次发送数据包时间
        /// </summary>
        public DateTime LastSendTime { get; set; }=DateTime.Now;

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tcpClient">已建立连接的TcpClient</param>
        /// <param name="connectionName">连接名</param>
        public Connection(TcpClient tcpClient, string connectionName)
        {
            this.TcpClient = tcpClient;
            this.ConnectionName = connectionName;
            this.NickName = this.ConnectionName.Split(':')[0];
            this.NetworkStream = this.TcpClient.GetStream();
            this.messageQueue = new ConcurrentQueue<Message>();
        }

        /// <summary>
        /// 中断连接并释放资源
        /// </summary>
        public void Stop()
        {
            TcpClient.Client.Disconnect(false);
            NetworkStream.Close();
            TcpClient.Close();
        }

        /// <summary>
        /// 解析消息
        /// </summary>
        /// <returns></returns>
        public async Task< Message> Parse()
        {
            Message message = new Message();
            //先读出前四个字节，即Message长度
            byte[] buffer = new byte[4];
            if (this.NetworkStream.DataAvailable)
            {
                int count = await this.NetworkStream.ReadAsync(buffer, 0, 4);
                if (count == 4)
                {
                    message.MessageLength = BitConverter.ToInt32(buffer, 0);
                }
                else
                    throw new Exception("网络流长度不正确");
            }
            else
                throw new Exception("目前网络不可读");
            //读出消息的其它字节
            buffer = new byte[message.MessageLength - 4];
            if (this.NetworkStream.DataAvailable)
            {
                int count = await this.NetworkStream.ReadAsync(buffer, 0, buffer.Length);
                if (count == message.MessageLength - 4)
                {
                    message.Command = (Message.CommandType)buffer[0];
                    message.MainVersion = buffer[1];
                    message.SecondVersion = buffer[2];

                    //读出消息体
                    message.MessageBody = SocketFactory.DefaultEncoding.GetString(buffer, 3, buffer.Length - 3);
                    message.ConnectionName = this.ConnectionName;

                    return message;
                }
                else
                    throw new Exception("网络流长度不正确");
            }
            else
                throw new Exception("目前网络不可读");
        }


    }
}
