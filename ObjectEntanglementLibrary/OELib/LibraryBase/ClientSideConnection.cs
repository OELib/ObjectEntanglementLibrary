using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;

namespace OELib.LibraryBase
{
    public class ClientSideConnection : Connection
    {
        private static int _connectionNo;
        protected string _remoteIp = "";
        protected int _remotePort;

        public ClientSideConnection(IFormatter customFormatter = null, ILogger logger = null, bool useCompression = false)
            : base(customFormatter, logger, useCompression)
        {
            Name = $"Cli. Conn. {_connectionNo++}";
            PingInterval = 5000;
        }

        private static TcpClient makeTcpClient(IPEndPoint ep)
        {
            var cli = new TcpClient();
            cli.Connect(ep);
            return cli;
        }

        public virtual bool Start(string ipAddress, int port)
        {
            _remoteIp = ipAddress;
            _remotePort = port;
            try
            {
                var cli = makeTcpClient(new IPEndPoint(IPAddress.Parse(ipAddress), port));
                return Start(cli);
            }
            catch
            {
                return false;
            }
        }
    }
}