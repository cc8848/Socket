using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SocketLibrary
{
    /// <summary>
    /// Connection ��ժҪ˵����
    /// </summary>
    public class Connection
    {
        #region ����

        /// <summary>
        ///  �ṩ����������ʵĻ���������
        /// </summary>
        public NetworkStream NetworkStream { get; private set; }

        /// <summary>
        /// ���ӵ�Key
        /// </summary>
        public string ConnectionName { get; private set; }

        /// <summary>
        /// ���ӱ���
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// �����ӵ���Ϣ����
        /// </summary>
        public ConcurrentQueue<Message> messageQueue { get; protected set; }

        /// <summary>
        /// TcpClient
        /// </summary>
        public TcpClient TcpClient { get; private set; }
        /// <summary>
        /// ���һ�η������ݰ�ʱ��
        /// </summary>
        public DateTime LastSendTime { get; set; }=DateTime.Now;

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tcpClient">�ѽ������ӵ�TcpClient</param>
        /// <param name="connectionName">������</param>
        public Connection(TcpClient tcpClient, string connectionName)
        {
            this.TcpClient = tcpClient;
            this.ConnectionName = connectionName;
            this.NickName = this.ConnectionName.Split(':')[0];
            this.NetworkStream = this.TcpClient.GetStream();
            this.messageQueue = new ConcurrentQueue<Message>();
        }

        /// <summary>
        /// �ж����Ӳ��ͷ���Դ
        /// </summary>
        public void Stop()
        {
            TcpClient.Client.Disconnect(false);
            NetworkStream.Close();
            TcpClient.Close();
        }

        /// <summary>
        /// ������Ϣ
        /// </summary>
        /// <returns></returns>
        public async Task< Message> Parse()
        {
            Message message = new Message();
            //�ȶ���ǰ�ĸ��ֽڣ���Message����
            byte[] buffer = new byte[4];
            if (this.NetworkStream.DataAvailable)
            {
                int count = await this.NetworkStream.ReadAsync(buffer, 0, 4);
                if (count == 4)
                {
                    message.MessageLength = BitConverter.ToInt32(buffer, 0);
                }
                else
                    throw new Exception("���������Ȳ���ȷ");
            }
            else
                throw new Exception("Ŀǰ���粻�ɶ�");
            //������Ϣ�������ֽ�
            buffer = new byte[message.MessageLength - 4];
            if (this.NetworkStream.DataAvailable)
            {
                int count = await this.NetworkStream.ReadAsync(buffer, 0, buffer.Length);
                if (count == message.MessageLength - 4)
                {
                    message.Command = (Message.CommandType)buffer[0];
                    message.MainVersion = buffer[1];
                    message.SecondVersion = buffer[2];

                    //������Ϣ��
                    message.MessageBody = SocketFactory.DefaultEncoding.GetString(buffer, 3, buffer.Length - 3);
                    message.ConnectionName = this.ConnectionName;

                    return message;
                }
                else
                    throw new Exception("���������Ȳ���ȷ");
            }
            else
                throw new Exception("Ŀǰ���粻�ɶ�");
        }


    }
}
