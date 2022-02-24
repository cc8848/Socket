using System;
using System.Collections.Concurrent;
using System.Threading;

namespace SocketLibrary
{
    /// <summary>
    /// SocketBase,Socketͨ�Ż���
    /// </summary>
    public abstract class SocketBase
    {
        #region ����

        /// <summary>
        /// �����ӵ�Socket
        /// </summary>
        public ConcurrentDictionary<string, Connection> Connections { get; protected set; }

        #endregion

        protected SocketBase()
        {
            this.Connections = new ConcurrentDictionary<string, Connection>();
        }

        protected Thread Listenningthread;

        protected void StartListenAndSend()
        {
            Listenningthread = new Thread(new ThreadStart(Listenning));
            Listenningthread.Start();
        }

        protected void EndListenAndSend()
        {

            Thread.Sleep(200);//�Է���Ϣû�з��꣬������
            foreach (var keyValue in this.Connections)
            {
                this.Connections.TryRemove(keyValue.Key, out var remConn);
                remConn.Stop();
            }
            Listenningthread.Abort();
        }

        protected virtual void Listenning()
        {
            while (true)
            {
                Thread.Sleep(200);
                foreach (var keyValue in this.Connections)
                {
                    //�������

                    if (!this.HeartbeatCheck(keyValue.Value))
                    {
                        this.Connections.TryRemove(keyValue.Key, out _);
                        continue;
                    }
                    try
                    {
                        this.Receive(keyValue.Value);//��������
                        this.Send(keyValue.Value); //��������
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

        #region �ܱ����ķ���

        /// <summary>
        /// ��������
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
        /// ��������
        /// </summary>
        /// <param name="connection"></param>
        private void Receive(Connection connection)
        {
            if (connection.NetworkStream.CanRead && connection.NetworkStream.DataAvailable)
            {
                Message message = connection.Parse();
                //����������ʱ���������¼�
                if (!message.Command.Equals(Message.CommandType.Seartbeat))
                {
                    this.OnMessageReceived(this, new MessageEventArgs(message, connection));
                }
            }
        }
        /// <summary>
        /// �������
        /// </summary>
        private bool HeartbeatCheck(Connection connection)
        {
            bool bol = false;
            byte[] buffer = new Message(Message.CommandType.Seartbeat, "���������ɺ���").ToBytes();
            try
            {
                lock (this)
                {
                    connection.NetworkStream.Write(buffer, 0, buffer.Length);
                    bol = true;
                }
            }
            catch (Exception ex) //�����Ѿ��Ͽ�
            {
                connection.NetworkStream.Close();
                ConCloseMessagesEventArgs ce = new ConCloseMessagesEventArgs(connection.ConnectionName, new ConcurrentQueue<Message>(connection.messageQueue), ex);
                this.OnConnectionClose(this, ce);
            }
            return bol;
        }

        #endregion

        #region ���ӹر��¼�
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
        /// ���ӹر��¼�
        /// </summary>
        public event ConCloseMessagesHandler ConnectionClose;
        private void OnConnectionClose(object sender, ConCloseMessagesEventArgs e)
        {
            if (ConnectionClose != null)
                this.ConnectionClose(sender, e);
        }
        #endregion

        #region ���ӽ����¼�
        public delegate void ConnectedEventArgs(object sender, Connection e);
        /// <summary>
        /// �����ӽ����¼�
        /// </summary>
        public event ConnectedEventArgs Connected;
        protected void OnConnected(object sender, Connection e)
        {
            if (Connected != null)
                this.Connected(sender, e);
        }
        #endregion




        #region Message�¼�
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
        /// ���յ���Ϣ�¼�
        /// </summary>
        public event MessageEventHandler MessageReceived;
        private void OnMessageReceived(object sender, MessageEventArgs e)
        {
            if (MessageReceived != null)
                this.MessageReceived(sender, e);
        }
        /// <summary>
        /// ��Ϣ�ѷ����¼�
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
