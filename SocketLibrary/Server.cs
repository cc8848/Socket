using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

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
        public Server( int port)
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
        }  public Server(IPAddress address, int port)
        {
            this.ipAddress = address;
            this.port = port;
        }

        // protected Thread _listenConnection;
        // protected CancellationTokenSource _listenConnection = new CancellationTokenSource();
        /// <summary>
        /// 打开监听
        /// </summary>
        public void StartServer()
        {
            _listener = new TcpListener(this.ipAddress, this.port);
            _listener.Start();
            //2022年3月2日 改为异步获取
            _listener.BeginAcceptTcpClient(this.OnAcceptConnection, _listener);
            //检测客户端链接
            // new TaskFactory().StartNew(Start, _listenConnection.Token);
            // _listenConnection = new Thread(Start);
            // _listenConnection.Start();

            this.StartListenAndSend();
        }

        private void OnAcceptConnection(IAsyncResult asyn)
        {
            // Get the listener that handles the client request.
            TcpListener listener = (TcpListener)asyn.AsyncState;

            TcpClient client = listener.EndAcceptTcpClient(asyn);
            string piEndPoint = client.Client.RemoteEndPoint.ToString();
            Connection connection = new Connection(client, piEndPoint);
            this.Connections.TryAdd(piEndPoint, connection);
            this.OnConnected(this, connection);

            listener.BeginAcceptTcpClient(this.OnAcceptConnection, listener);
        }

        // private async Task Start()
        // {
        //     try
        //     {
        //         while (true)
        //         {
        //             if (_listener.Pending())
        //             {
        //                 TcpClient client = await _listener.AcceptTcpClientAsync();
        //                 string piEndPoint = client.Client.RemoteEndPoint.ToString();
        //                 Connection connection = new Connection(client, piEndPoint);
        //                 this.Connections.TryAdd(piEndPoint, connection);
        //                 this.OnConnected(this, connection);
        //             }
        //
        //             await Task.Delay(200);
        //         }
        //     }
        //     catch
        //     {
        //     }
        // }

        /// <summary>
        /// 关闭监听
        /// </summary>
        public void StopServer()
        {
            // _listenConnection.Cancel(false);
            // _listenConnection.Abort();
            this.EndListenAndSend();
        }
    }
}
