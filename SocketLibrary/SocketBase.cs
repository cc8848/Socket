using System;
using System.Collections.Concurrent;
using System.Threading;

namespace SocketLibrary
{
    /// <summary>
    /// SocketBase,Socket通信基类
    /// </summary>
    public abstract class SocketBase
    {
        #region 属性

        /// <summary>
        /// 已连接的Socket
        /// </summary>
        public ConcurrentDictionary<string, Connection> Connections
        {
            get { return _connections; }
            protected set { _connections = value; }
        }
        protected ConcurrentDictionary<string, Connection> _connections;

        #endregion

        protected SocketBase()
        {
            this._connections = new ConcurrentDictionary<string, Connection>();
        }

        protected Thread _listenningthread;

        protected void StartListenAndSend()
        {
            _listenningthread = new Thread(new ThreadStart(Listenning));
            _listenningthread.Start();
        }

        protected void EndListenAndSend()
        {

            Thread.Sleep(200);//以防消息没有发完，或收完
            foreach (var keyValue in this._connections)
            {
                Connection remConn;
                this._connections.TryRemove(keyValue.Key, out remConn);
                remConn.Stop();
            }
            _listenningthread.Abort();
        }

        protected virtual void Listenning()
        {
            while (true)
            {
                Thread.Sleep(200);
                foreach (var keyValue in this._connections)
                {
                    //心跳检测

                    if (!this.HeartbeatCheck(keyValue.Value))
                    {
                        Connection remConn;
                        this._connections.TryRemove(keyValue.Key, out remConn);
                        continue;
                    }
                    try
                    {
                        this.Receive(keyValue.Value);//接收数据
                        this.Send(keyValue.Value); //发送数据
                    }
                    catch (Exception ex)
                    {
                        keyValue.Value.NetworkStream.Close();
                        ConCloseMessagesEventArgs ce = new ConCloseMessagesEventArgs(keyValue.Value.ConnectionName, new ConcurrentQueue<Message>(keyValue.Value.messageQueue), ex);
                        this.OnConnectionClose(this, ce);
                    }
                }
            }
        }

        #region 受保护的方法

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="connection"></param>
        private void Send(Connection connection)
        {
            if (connection.NetworkStream.CanWrite)
            {
                Message message;
                while (connection.messageQueue.TryDequeue(out message))
                {
                    byte[] buffer = message.ToBytes();
                    lock (this)
                    {
                        connection.NetworkStream.Write(buffer, 0, buffer.Length);
                        message.Sent = true;
                    }
                    this.OnMessageSent(this, new MessageEventArgs(message, connection));
                }
            }
        }
        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="connection"></param>
        private void Receive(Connection connection)
        {
            if (connection.NetworkStream.CanRead && connection.NetworkStream.DataAvailable)
            {
                Message message = connection.Parse();
                //不是心跳包时触发接收事件
                if (!message.Command.Equals(Message.CommandType.Seartbeat))
                {
                    this.OnMessageReceived(this, new MessageEventArgs(message, connection));
                }
            }
        }
        /// <summary>
        /// 心跳检测
        /// </summary>
        private bool HeartbeatCheck(Connection connection)
        {
            bool bol = false;
            byte[] buffer = new Message(Message.CommandType.Seartbeat, "心跳包，可忽略").ToBytes();
            try
            {
                lock (this)
                {
                    connection.NetworkStream.Write(buffer, 0, buffer.Length);
                    bol = true;
                }
            }
            catch (Exception ex) //连接已经断开
            {
                connection.NetworkStream.Close();
                ConCloseMessagesEventArgs ce = new ConCloseMessagesEventArgs(connection.ConnectionName, new ConcurrentQueue<Message>(connection.messageQueue), ex);
                this.OnConnectionClose(this, ce);
            }
            return bol;
        }

        #endregion

        #region 连接关闭事件
        public class ConCloseMessagesEventArgs : EventArgs
        {
            public string ConnectionName;
            public ConcurrentQueue<Message> MessageQueue;
            public Exception Exception;
            public ConCloseMessagesEventArgs(string connectionName, ConcurrentQueue<Message> messageQueue, Exception exception)
            {
                this.ConnectionName = connectionName;
                this.MessageQueue = messageQueue;
                this.Exception = exception;
            }
        }
        public delegate void ConCloseMessagesHandler(object sender, ConCloseMessagesEventArgs e);
        /// <summary>
        /// 连接关闭事件
        /// </summary>
        public event ConCloseMessagesHandler ConnectionClose;
        private void OnConnectionClose(object sender, ConCloseMessagesEventArgs e)
        {
            if (ConnectionClose != null)
                this.ConnectionClose(sender, e);
        }
        #endregion

        #region 连接接入事件
        public delegate void ConnectedEventArgs(object sender, Connection e);
        /// <summary>
        /// 新连接接入事件
        /// </summary>
        public event ConnectedEventArgs Connected;
        protected void OnConnected(object sender, Connection e)
        {
            if (Connected != null)
                this.Connected(sender, e);
        }
        #endregion

        #region Message事件

        public class MessageEventArgs : EventArgs
        {
            public Message Message;
            public Connection Connecction;
            public MessageEventArgs(Message message, Connection connection)
            {
                this.Message = message;
                this.Connecction = connection;
            }
        }
        public delegate void MessageEventHandler(object sender, MessageEventArgs e);
        /// <summary>
        /// 接收到消息事件
        /// </summary>
        public event MessageEventHandler MessageReceived;
        private void OnMessageReceived(object sender, MessageEventArgs e)
        {
            if (MessageReceived != null)
                this.MessageReceived(sender, e);
        }
        /// <summary>
        /// 消息已发出事件
        /// </summary>
        public event MessageEventHandler MessageSent;
        private void OnMessageSent(object sender, MessageEventArgs e)
        {
            if (MessageSent != null)
                this.MessageSent(sender, e);
        }

        #endregion

    }
}
