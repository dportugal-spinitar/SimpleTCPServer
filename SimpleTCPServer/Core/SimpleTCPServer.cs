using System;
using System.Collections.Generic;
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
	[Serializable]
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
        private readonly IPEndPoint _endpoint;
        /// <summary>
        /// Starts everything in the server
        /// </summary>
        public async Task StartAsync()
        {
			if (isStarted)
				return;

			StartServerCancellationToken = new CancellationTokenSource();
			_taskholders.Add(++_tasks, new TaskHolder(StartServerCancellationToken, TaskType.User));
			Task b = Task.Factory.StartNew(() => _onStart(), StartServerCancellationToken.Token);

			isStarted = true;
			await b;
        }

        private CancellationTokenSource StartServerCancellationToken;

        /// <summary>
        /// Stop server
        /// </summary>
        /// <returns></returns>
        public async Task StopAsync()
        {
            if (isStarted)
            {                               
                foreach (var item in _taskholders)
                {
                    if (item.Value != null)
                        if (!item.Value.TokenSource.IsCancellationRequested)
                            item.Value.TokenSource.Cancel();
                }
                _taskholders.Clear();

                foreach (TcpClient client in _clients)
                {
                    if (client != null)
                    {
                        if (client.Connected)
                        {
                            client.GetStream().Close();
                            client.Close();
                        }
                        client.Dispose();
                    }
                }

                StartServerCancellationToken.Cancel();
                _listener.Stop();
                isStarted = false;
                await _log("Stopped","Server", LogMessageType.ServerStopped);                
            }
        }

		private bool isStarted = false;
		private long _tasks = 0;
        /// <summary>
        /// The config for the tcp server
        /// </summary>
        public TCPServerConfig Config { get; private set; } = new TCPServerConfig
        {
            BytesSize = 1024,
			KeepListOfClients = false
        };

		private readonly Dictionary<long, TaskHolder> _taskholders = new Dictionary<long, TaskHolder>();
		/// <summary>
		/// The list of connected clients
		/// </summary>
		public IReadOnlyList<TcpClient> Clients
		{
			get
			{
				if (Config.KeepListOfClients)
					return _clients;
				return null;
			}
		}
		private readonly List<TcpClient> _clients = new List<TcpClient>();

		private TcpListener _listener;
        private async void _onStart()
        {
            _listener = new TcpListener(_endpoint);
			_listener.Start();
			
            await _log("Ready", "Server", LogMessageType.ServerReady);
            await OnReady(_listener);
			var cancellation = _taskholders[_tasks];
            while (true)
            {
                var client = await _listener.AcceptTcpClientAsync();
				if (cancellation.TokenSource.IsCancellationRequested)
				{
					_listener.Stop();
					return;
				}

				await _log("Client Connected", ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString(), LogMessageType.ClientConnected);
                await OnClientConnect(client);

				CancellationTokenSource tokenSource = new CancellationTokenSource();

				_tasks++;
				_taskholders.Add(_tasks, new TaskHolder(tokenSource, TaskType.Receiving));
				Task task = Task.Factory.StartNew(() => _userThread(client, _tasks), tokenSource.Token);
			
				await task;
			}
        }
       
		[Serializable]
        private struct LogMessage : ILogMessage
        {
            public string Content { get; set; }

            public string Source { get; set; }

            public LogMessageType Type { get; set; }
        }

		/// <summary>
		/// Stops a task with that taskid
		/// </summary>
		/// <param name="taskId">The task id</param>
		public void StopTask(long taskId)
		{
			if (!_taskholders.ContainsKey(taskId))
				return;
			var value = _taskholders[taskId];
			value.TokenSource.Cancel();
			value.Dispose();
			_taskholders.Remove(taskId);
		}

        private async void _userThread(TcpClient sender, long task)
        {
            TcpClient mClient = sender;
			_clients.Add(mClient);
			var cancellation = _taskholders[task];

			async Task cancel()
			{
				await _log("Client Disconnected", ((IPEndPoint)mClient.Client.RemoteEndPoint).Address.ToString(), LogMessageType.ClientDisconnected);
				_clients.Remove(mClient);
				await OnClientLeave(mClient);
				_taskholders.Remove(task);
			};

            while (true)
            {
                try
                {
					if (cancellation.TokenSource.IsCancellationRequested)
					{
						await cancel();
						return;
					}

					var stream = mClient.GetStream();
                    byte[] bytes = new byte[Config.BytesSize];
                    await stream.ReadAsync(bytes, 0, bytes.Length);

					await _log("Bytes received", ((IPEndPoint)mClient.Client.RemoteEndPoint).Address.ToString(), LogMessageType.BytesReceived);
                    await BytesReceived(mClient, stream, bytes);
                }
                catch
                {
					await cancel();
					return;
                }
            }
        }
        private async Task _log(string content, string source, LogMessageType type)
        {
            await Log(new LogMessage { Content = content, Source = source, Type = type });
        }

		#region Events

		/// <summary>
		/// The log Task
		/// </summary>
		public event Func<ILogMessage, Task> Log = async (ILogMessage blank) => { await Task.CompletedTask; };
        /// <summary>
        /// The event thats fired when the server is listening
        /// </summary>
        public event Func<TcpListener, Task> OnReady = async (TcpListener blank) => { await Task.CompletedTask; };
        /// <summary>
        /// The event thats fired when a client connects
        /// </summary>
        public event Func<TcpClient, Task> OnClientConnect = async (TcpClient blank) => { await Task.CompletedTask; };
        /// <summary>
        /// The event thats fired when a client leaves
        /// </summary>
        public event Func<TcpClient, Task> OnClientLeave = async (TcpClient blank) => { await Task.CompletedTask; };
        /// <summary>
        /// The event thats fired when bytes are received
        /// </summary>
        public event Func<TcpClient, NetworkStream, byte[], Task> BytesReceived = async (TcpClient blank1, NetworkStream blank2, byte[] blank3) => { await Task.CompletedTask; };

		#endregion

	}


}