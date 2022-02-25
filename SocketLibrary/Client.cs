using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

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
        // protected Thread _listenningClientThread;
        protected CancellationTokenSource ListenningClientCancellationTokenSource;

        /// <summary>
        /// 连接的Key
        /// </summary>
        public string ClientName { get; }

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
            this.ClientName = ipAddress + ":" + port;
        }

        /// <summary>
        /// 打开链接
        /// </summary>
        public void StartClient()
        {
            this.StartListenAndSend();//开启父类的监听线程
            ListenningClientCancellationTokenSource= new CancellationTokenSource();
            new TaskFactory(ListenningClientCancellationTokenSource.Token).StartNew(Start);
        }
        /// <summary>
        /// 关闭连接并释放资源
        /// </summary>
        public void StopClient()
        {
            //缺少通知给服务端 自己主动关闭了
            ListenningClientCancellationTokenSource.Cancel(false);
            this.EndListenAndSend();
        }
        //循环检查链接状态,如果断开重新链接
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
                    { //定义连接失败事件
                    }
                }

                await Task.Delay(200);
            }
        }
    }
}
