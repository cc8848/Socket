using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;

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
        public ConcurrentDictionary<string, Connection> Connections { get; protected set; }
        /// <summary>
        /// 心跳检查间隔秒数
        /// </summary>
        public int HeartbeatInterval { get; set; } = 5;

        #endregion

        protected SocketBase()
        {
            this.Connections = new ConcurrentDictionary<string, Connection>();
        }

        public CancellationTokenSource ListenningCancellationToken;

        protected void StartListenAndSend()
        {
            ListenningCancellationToken = new CancellationTokenSource();
            new TaskFactory(ListenningCancellationToken.Token).StartNew(Listenning);

        }

        protected void EndListenAndSend()
        {

            Thread.Sleep(200);//以防消息没有发完，或收完
            foreach (var keyValue in this.Connections)
            {
                this.Connections.TryRemove(keyValue.Key, out var remConn);
                remConn.Stop();
            }
            ListenningCancellationToken.Cancel(false);
        }

        protected virtual async Task Listenning()
        {
            while (true)
            {
                foreach (var keyValue in this.Connections)
                {
                    //心跳检测
                    if (!await this.HeartbeatCheck(keyValue.Value))
                    {
                        continue;
                    }

                    try
                    {
                        await this.Receive(keyValue.Value);//接收数据
                        await this.Send(keyValue.Value); //发送数据
                    }
                    catch (Exception ex)
                    {
                        keyValue.Value.NetworkStream.Close();
                        ConCloseMessagesEventArgs ce = new ConCloseMessagesEventArgs(keyValue.Value.ConnectionName, new ConcurrentQueue<Message>(keyValue.Value.messageQueue), ex);
                        await this.OnConnectionClose(this, ce);
                    }
                }
            }
        }

        #region 受保护的方法

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="connection"></param>
        private async Task Send(Connection connection)
        {
            if (connection.NetworkStream.CanWrite)
            {
                while (connection.messageQueue.TryDequeue(out var message))
                {
                    await WriteAsync(connection, message);
                    await this.OnMessageSent(this, new MessageEventArgs(message, connection));
                }
            }
        }
        private readonly AsyncLock _writeAsyncLock = new AsyncLock();

        private async Task WriteAsync(Connection connection, Message message)
        {
            byte[] buffer = message.ToBytes();
            using (await _writeAsyncLock.LockAsync())
            {
                await connection.NetworkStream.WriteAsync(buffer, 0, buffer.Length);
                message.Sent = true;
                connection. LastSendTime=DateTime.Now;
            }
        }

        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="connection"></param>
        private async Task Receive(Connection connection)
        {
            if (connection.NetworkStream.CanRead && connection.NetworkStream.DataAvailable)
            {
                Message message = await connection.Parse();
                //不是心跳包时触发接收事件
                if (!message.Command.Equals(Message.CommandType.Seartbeat))
                {
                    await this.OnMessageReceived(this, new MessageEventArgs(message, connection));
                }
            }
        }

        /// <summary>
        /// 心跳检测
        /// </summary>
        /// <param name="connection"></param>
        /// <returns>是否联通</returns>
        private async Task<bool> HeartbeatCheck(Connection connection)
        {
            if (connection.LastSendTime.AddMilliseconds(HeartbeatInterval * 1000) <= DateTime.Now)
            {
                var message = new Message(Message.CommandType.Seartbeat, "心跳包");
                try
                {
                    await WriteAsync(connection, message);
                }
                catch (Exception ex) //连接已经断开
                {
                    connection.NetworkStream.Close();
                    ConCloseMessagesEventArgs ce = new ConCloseMessagesEventArgs(connection.ConnectionName, new ConcurrentQueue<Message>(connection.messageQueue), ex);
                    await this.OnConnectionClose(this, ce);
                    return false;

                }
            }
         
           
            return true;
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
        public delegate Task ConCloseMessagesHandler(object sender, ConCloseMessagesEventArgs e);
        /// <summary>
        /// 连接关闭事件
        /// </summary>
        public event ConCloseMessagesHandler ConnectionClose;
        private async Task OnConnectionClose(object sender, ConCloseMessagesEventArgs e)
        {
            this.Connections.TryRemove(e.ConnectionName, out _);
            if (ConnectionClose != null)
               await this.ConnectionClose(sender, e);
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
        public delegate Task MessageEventHandler(object sender, MessageEventArgs e);
        /// <summary>
        /// 接收到消息事件
        /// </summary>
        public event MessageEventHandler MessageReceived;
        private async Task OnMessageReceived(object sender, MessageEventArgs e)
        {
            if (MessageReceived != null)
                await this.MessageReceived(sender, e);
        }
        /// <summary>
        /// 消息已发出事件
        /// </summary>
        public event MessageEventHandler MessageSent;
        private async Task OnMessageSent(object sender, MessageEventArgs e)
        {
            if (MessageSent != null)
                await this.MessageSent(sender, e);
        }

        #endregion

    }
}
