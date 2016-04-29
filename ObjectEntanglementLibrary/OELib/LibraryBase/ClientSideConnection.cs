using System.Net;
using System.Net.Sockets;

namespace OELib.LibraryBase
{
    public class ClientSideConnection : Connection
    {
        private static int connectionNo = 0;
        protected string remoteIP = "";
        protected int remotePort = 0;


        private static TcpClient makeTcpClient(IPEndPoint ep)
        {
            TcpClient cli = new TcpClient();
            cli.Connect(ep);
            return cli;
        }

        public ClientSideConnection()
            : base()
        {
            Name = $"Cli. Conn. {connectionNo++}";
            PingInterval = 5000;
        }

        public virtual bool Start(string IpAddress, int port)
        {
            remoteIP = IpAddress;
            remotePort = port;
            try
            {
                var cli = makeTcpClient(new IPEndPoint(IPAddress.Parse(IpAddress), port));
                return Start(cli);
            }
            catch
            {
                return false;
            }
        }



    }
}
