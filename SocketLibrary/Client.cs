using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SocketLibrary
{
    /// <summary>
    /// Socket�Ŀͻ���
    /// </summary>
    public class Client : SocketBase
    {
        /// <summary>
        ///  ��ʱʱ��
        /// </summary>
        public const int CONNECTTIMEOUT = 10;
        private TcpClient client;
        private IPAddress ipAddress;
        private int port;
        // protected Thread _listenningClientThread;
        protected CancellationTokenSource ListenningClientCancellationTokenSource;

        /// <summary>
        /// ���ӵ�Key
        /// </summary>
        public string ClientName { get; }

        /// <summary>
        /// ��ʼ��
        /// </summary>
        /// <param name="ipaddress">��ַ</param>
        /// <param name="port">�˿�</param>
        public Client(string ipaddress, int port)
            : this(IPAddress.Parse(ipaddress), port)
        {
        }
        /// <summary>
        ///��ʼ�� 
        /// </summary>
        /// <param name="ipaddress">��ַ</param>
        /// <param name="port">�˿�</param>
        public Client(IPAddress ipaddress, int port)
        {
            this.ipAddress = ipaddress;
            this.port = port;
            this.ClientName = ipAddress + ":" + port;
        }

        /// <summary>
        /// ������
        /// </summary>
        public void StartClient()
        {
            this.StartListenAndSend();//��������ļ����߳�
            ListenningClientCancellationTokenSource= new CancellationTokenSource();
            new TaskFactory(ListenningClientCancellationTokenSource.Token).StartNew(Start);
        }
        /// <summary>
        /// �ر����Ӳ��ͷ���Դ
        /// </summary>
        public void StopClient()
        {
            //ȱ��֪ͨ������� �Լ������ر���
            ListenningClientCancellationTokenSource.Cancel(false);
            this.EndListenAndSend();
        }
        //ѭ���������״̬,����Ͽ���������
        private async Task Start()
        {
            while (true)
            {
                if (!this.Connections.ContainsKey(this.ClientName))
                {
                    try
                    {
                        client = new TcpClient();
                        client.SendTimeout = CONNECTTIMEOUT;
                        client.ReceiveTimeout = CONNECTTIMEOUT;
                        await client.ConnectAsync(ipAddress, port);
                        var connection = new Connection(client, this.ClientName);
                        this.Connections.TryAdd(this.ClientName, connection);
                        this.OnConnected(this, connection);
                    }
                    catch (Exception e)
                    { //��������ʧ���¼�
                    }
                }

                await Task.Delay(200);
            }
        }
    }
}
