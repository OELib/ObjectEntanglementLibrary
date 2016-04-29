using System.Net.Sockets;

namespace OELib.LibraryBase
{
    public class ServerSideConnection : Connection
    {
        private static int connectionNo = 0;
        public ServerSideConnection(TcpClient client)
            : this()
        {
            Start(client);
        }

        protected ServerSideConnection() // in case we need to do something else in constructor before starting the Connection, so if you use this ctor, you must start manually.
        {
            Name = $"Srv. conn. {connectionNo++}";
            PingInterval = 7000;
        }

    }
}
