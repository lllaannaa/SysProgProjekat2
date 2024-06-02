using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SysProgProjekat2
{
    class Server
    {
        private readonly string _ipAddress;
        private readonly int _port;
        private readonly string _rootDirectory;
        private readonly TcpListener _listener;

        public Server(string ipAddress, int port, string rootDirectory)
        {
            _ipAddress = ipAddress;
            _port = port;
            _rootDirectory = rootDirectory;
            _listener = new TcpListener(IPAddress.Parse(ipAddress), port);
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            _listener.Start();
            Console.WriteLine("Server je pokrenut...");

            while (!cancellationToken.IsCancellationRequested)
            {
                TcpClient client = await _listener.AcceptTcpClientAsync();
                _ = Task.Run(() => new RequestProcessor(_rootDirectory).ProcessRequestAsync(client, cancellationToken));
            }
        }
    }
}
