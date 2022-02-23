using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

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
        protected Thread _listenningClientThread;

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
            _listenningClientThread = new Thread(new ThreadStart(Start));
            _listenningClientThread.Start();
        }
        /// <summary>
        /// �ر����Ӳ��ͷ���Դ
        /// </summary>
        public void StopClient()
        {
            //ȱ��֪ͨ������� �Լ������ر���
            _listenningClientThread.Abort();
            this.EndListenAndSend();
        }

        private void Start()
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
                        client.Connect(ipAddress, port);
                        this._connections.TryAdd(this.ClientName, new Connection(client, this.ClientName));
                    }
                    catch (Exception e)
                    { //��������ʧ���¼�
                    }
                }
                Thread.Sleep(200);
            }
        }
    }
}
