using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace SocketLibrary
{
    /// <summary>
    /// Connection 的摘要说明。
    /// </summary>
    public class Connection
    {
        #region 属性

        private NetworkStream _networkStream;
        /// <summary>
        ///  提供用于网络访问的基础数据流
        /// </summary>
        public NetworkStream NetworkStream
        {
            get { return _networkStream; }
            private set { _networkStream = value; }
        }
        private string _connectionName;
        /// <summary>
        /// 连接的Key
        /// </summary>
        public string ConnectionName
        {
            get { return _connectionName; }
            private set { _connectionName = value; }
        }
        private string _nickName;
        /// <summary>
        /// 连接别名
        /// </summary>
        public string NickName
        {
            get { return _nickName; }
            set { _nickName = value; }
        }
        /// <summary>
        /// 此链接的消息队列
        /// </summary>
        public ConcurrentQueue<Message> messageQueue
        {
            get { return _messageQueue; }
            private set { _messageQueue = value; }
        }
        protected ConcurrentQueue<Message> _messageQueue;

        private TcpClient _tcpClient;
        /// <summary>
        /// TcpClient
        /// </summary>
        public TcpClient TcpClient
        {
            get { return _tcpClient; }
            private set { _tcpClient = value; }
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tcpClient">已建立连接的TcpClient</param>
        /// <param name="connectionName">连接名</param>
        public Connection(TcpClient tcpClient, string connectionName)
        {
            this._tcpClient = tcpClient;
            this._connectionName = connectionName;
            this.NickName = this._connectionName.Split(':')[0];
            this._networkStream = this._tcpClient.GetStream();
            this._messageQueue = new ConcurrentQueue<Message>();
        }

        /// <summary>
        /// 中断连接并释放资源
        /// </summary>
        public void Stop()
        {
            _tcpClient.Client.Disconnect(false);
            _networkStream.Close();
            _tcpClient.Close();
        }

        /// <summary>
        /// 解析消息
        /// </summary>
        /// <returns></returns>
        public Message Parse()
        {
            Message message = new Message();
            //先读出前四个字节，即Message长度
            byte[] buffer = new byte[4];
            if (this._networkStream.DataAvailable)
            {
                int count = this._networkStream.Read(buffer, 0, 4);
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
            if (this._networkStream.DataAvailable)
            {
                int count = this._networkStream.Read(buffer, 0, buffer.Length);
                if (count == message.MessageLength - 4)
                {
                    message.Command = (Message.CommandType)buffer[0];
                    message.MainVersion = buffer[1];
                    message.SecondVersion = buffer[2];

                    //读出消息体
                    message.MessageBody = SocketFactory.DefaultEncoding.GetString(buffer, 3, buffer.Length - 3);
                    message.ConnectionName = this._connectionName;

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
