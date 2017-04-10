using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SocketLibrary
{
    /// <summary>
    /// Socket的客户端
    /// </summary>
    public class Client : SocketBase
    {
        /// <summary>
        ///  超时时间
        /// </summary>
        public const int CONNECTTIMEOUT = 10;
        private TcpClient client;
        private IPAddress ipAddress;
        private int port;
        protected Thread _listenningClientThread;
        private string _clientName;
        /// <summary>
        /// 连接的Key
        /// </summary>
        public string ClientName
        {
            get { return _clientName; }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="ipaddress">地址</param>
        /// <param name="port">端口</param>
        public Client(string ipaddress, int port)
            : this(IPAddress.Parse(ipaddress), port)
        {
        }
        /// <summary>
        ///初始化 
        /// </summary>
        /// <param name="ipaddress">地址</param>
        /// <param name="port">端口</param>
        public Client(IPAddress ipaddress, int port)
        {
            this.ipAddress = ipaddress;
            this.port = port;
            this._clientName = ipAddress + ":" + port;
        }

        /// <summary>
        /// 打开链接
        /// </summary>
        public void StartClient()
        {
            this.StartListenAndSend();//开启父类的监听线程
            _listenningClientThread = new Thread(new ThreadStart(Start));
            _listenningClientThread.Start();
        }
        /// <summary>
        /// 关闭连接并释放资源
        /// </summary>
        public void StopClient()
        {
            //缺少通知给服务端 自己主动关闭了
            _listenningClientThread.Abort();
            this.EndListenAndSend();
        }

        private void Start()
        {
            while (true)
            {
                if (!this.Connections.ContainsKey(this._clientName))
                {
                    try
                    {
                        client = new TcpClient();
                        client.SendTimeout = CONNECTTIMEOUT;
                        client.ReceiveTimeout = CONNECTTIMEOUT;
                        client.Connect(ipAddress, port);
                        this._connections.TryAdd(this._clientName, new Connection(client, this._clientName));
                    }
                    catch (Exception e)
                    { //定义连接失败事件
                    }
                }
                Thread.Sleep(200);
            }
        }
    }
}
