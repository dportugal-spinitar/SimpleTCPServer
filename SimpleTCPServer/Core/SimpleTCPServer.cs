using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using SimpleTCPServer.Logging;
using static SimpleTCPServer.Extensions.Methods;
namespace SimpleTCPServer.Core
{
    /// <summary>
    /// The main class for creating a simple tcp server
    /// </summary>
    public class TCPServer
    {
        /// <summary>
        /// Initializes this class with an IPEndpoint and config
        /// </summary>
        /// <param name="endpoint">The IPEndpoint</param>
        /// <param name="config">The configuration</param>
        public TCPServer(IPEndPoint endpoint, TCPServerConfig config)
        {
            _endpoint = endpoint;
            Config = config;
        }
        /// <summary>
        /// Initializes this class with an IPEndpoint
        /// </summary>
        /// <param name="endpoint">The ipendpoint</param>
        public TCPServer(IPEndPoint endpoint)
        {
            _endpoint = endpoint;
        }
        /// <summary>
        /// Initializes this class with an IPAddress and port
        /// </summary>
        /// <param name="address">The IPAddress</param>
        /// <param name="port">The port</param>
        public TCPServer(IPAddress address, int port)
        {
            _endpoint = new IPEndPoint(address, port);
        }
        /// <summary>
        /// Intitializes this class with an IPAddress, port and config
        /// </summary>
        /// <param name="address">The ip address</param>
        /// <param name="port">The port</param>
        /// <param name="config">The configuration</param>
        public TCPServer(IPAddress address, int port, TCPServerConfig config)
        {
            _endpoint = new IPEndPoint(address, port);
            Config = config;
        }
        /// <summary>
        /// Initializes this class with a string address and a port
        /// </summary>
        /// <param name="address">The string ip address</param>
        /// <param name="port">The port</param>
        public TCPServer(string address, int port)
        {
            _endpoint = new IPEndPoint(IPAddress.Parse(address), port);
        }
        /// <summary>
        /// Initializes this class with a string address, a port and a config
        /// </summary>
        /// <param name="address">The string ip address</param>
        /// <param name="port">The port</param>
        /// <param name="config">The configuration</param>
        public TCPServer(string address, int port, TCPServerConfig config)
        {
            _endpoint = new IPEndPoint(IPAddress.Parse(address), port);
            Config = config;
        }
        /// <summary>
        /// Initializes this class with an IPEndpoint string
        /// </summary>
        /// <param name="ipendpoint">The IPEndpoint string</param>
        public TCPServer(string ipendpoint)
        {
            _endpoint = CreateIPEndPoint(ipendpoint);
        }
        /// <summary>
        /// Initializes this class with a string which contains the ip and port and a config
        /// </summary>
        /// <param name="ipendpoint">The IPEndpoint string</param>
        /// <param name="config">The configuration</param>
        public TCPServer(string ipendpoint, TCPServerConfig config)
        {
            _endpoint = CreateIPEndPoint(ipendpoint);
            Config = config;
        }
        /// <summary>
        /// The ip endpoint
        /// </summary>
        public IPEndPoint Endpoint { get => _endpoint; }
        private IPEndPoint _endpoint;
        /// <summary>
        /// Starts everything in the server
        /// </summary>
        public void Start()
        {
            Thread tcpServerRunThread = new Thread(_onStart);
            tcpServerRunThread.Start();
        }
        /// <summary>
        /// The config for the tcp server
        /// </summary>
        public TCPServerConfig Config { get; set; } = new TCPServerConfig
        {
            BytesSize = 1024,
        };
        private async void _onStart()
        {
            TcpListener server = new TcpListener(_endpoint);
            server.Start();

            await _log("Ready", "Server", LogMessageType.ServerReady);
            await OnReady(server);

            while (true)
            {
                var client = await server.AcceptTcpClientAsync();

                await _log("Client Connected", ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString(), LogMessageType.ClientConnected);
                await OnClientConnect(client);

                Thread TCPHandlerThread = new Thread(new ParameterizedThreadStart(_userThread));
                TCPHandlerThread.Start(client);
            }
        }
       
        private class LogMessage : ILogMessage
        {
            public string Content { get; set; }

            public string Source { get; set; }

            public LogMessageType Type { get; set; }
        }

        private async void _userThread(object sender)
        {
            TcpClient mClient = (TcpClient)sender;
            while (true)
            {
                try
                {
                    var stream = mClient.GetStream();
                    byte[] bytes = new byte[Config.BytesSize];
                    await stream.ReadAsync(bytes, 0, bytes.Length);

                    await _log("Bytes received", ((IPEndPoint)mClient.Client.RemoteEndPoint).Address.ToString(), LogMessageType.BytesReceived);
                    await BytesReceived(mClient, stream, bytes);
                }
                catch
                {
                    await _log("Client Disconnected", ((IPEndPoint)mClient.Client.RemoteEndPoint).Address.ToString(), LogMessageType.ClientDisconnected);
                    await OnClientLeave(mClient);
                    return;
                }
            }
        }
        private async Task _log(string content, string source, LogMessageType type)
        {
            await Log(new LogMessage { Content = content, Source = source, Type = type });
        }

        /// <summary>
        /// The log Task
        /// </summary>
        public Func<ILogMessage, Task> Log { set; get; } = async (ILogMessage blank) => { await Task.CompletedTask; };
        /// <summary>
        /// The event thats fired when the server is listening
        /// </summary>
        public Func<TcpListener, Task> OnReady { set; get; } = async (TcpListener blank) => { await Task.CompletedTask; };
        /// <summary>
        /// The event thats fired when a client connects
        /// </summary>
        public Func<TcpClient, Task> OnClientConnect { set; get; } = async (TcpClient blank) => { await Task.CompletedTask; };
        /// <summary>
        /// The event thats fired when a client leaves
        /// </summary>
        public Func<TcpClient, Task> OnClientLeave { set; get; } = async (TcpClient blank) => { await Task.CompletedTask; };
        /// <summary>
        /// The event thats fired when bytes are received
        /// </summary>
        public Func<TcpClient, NetworkStream, byte[], Task> BytesReceived { set; get; } = async (TcpClient blank1, NetworkStream blank2, byte[] blank3) => { await Task.CompletedTask; };
    }

}
