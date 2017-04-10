using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SocketLibrary
{
    /// <summary>
    /// Socket服务端
    /// </summary>
    public class Server : SocketBase
    {
        private TcpListener _listener;
        private IPAddress ipAddress;
        private int port;

        /// <summary>
        /// 默监听所有IP的此端口
        /// </summary>
        /// <param name="port">端口号</param>
        public Server(int port)
        {
            this.ipAddress = IPAddress.Any;
            this.port = port;
        }
        /// <summary>
        /// 监听此IP的此端口
        /// </summary>
        /// <param name="ip">IP</param>
        /// <param name="port">端口号</param>
        public Server(string ip, int port)
        {
            this.ipAddress = IPAddress.Parse(ip);
            this.port = port;
        }

        protected Thread _listenConnection;
        /// <summary>
        /// 打开监听
        /// </summary>
        public void StartServer()
        {
            _listener = new TcpListener(this.ipAddress, this.port);
            _listener.Start();

            _listenConnection = new Thread(new ThreadStart(Start));
            _listenConnection.Start();

            this.StartListenAndSend();
        }

        private void Start()
        {
            try
            {
                while (true)
                {
                    if (_listener.Pending())
                    {
                        TcpClient client = _listener.AcceptTcpClient();
                        string piEndPoint = client.Client.RemoteEndPoint.ToString();
                        Connection connection = new Connection(client, piEndPoint);
                        this._connections.TryAdd(piEndPoint, connection);
                        this.OnConnected(this, connection);
                    }
                    Thread.Sleep(200);
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// 关闭监听
        /// </summary>
        public void StopServer()
        {
            _listenConnection.Abort();
            this.EndListenAndSend();
        }
    }
}
