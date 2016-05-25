using System.Net.Sockets;
using System.Runtime.Serialization;

namespace OELib.LibraryBase
{
    public class ServerSideConnection : Connection
    {
        private static int connectionNo = 0;

        public ServerSideConnection(TcpClient client, IFormatter customFormatter = null, ILogger logger = null, bool useCompression = false)
            : this(customFormatter, logger, useCompression)
        {
            Start(client);
        }

        protected ServerSideConnection(IFormatter customFormatter = null, ILogger logger = null, bool useCompression = false) // in case we need to do something else in constructor before starting the Connection, so if you use this ctor, you must start manually.
            : base(customFormatter, logger, useCompression)
        {
            Name = $"Srv. conn. {connectionNo++}";
            PingInterval = 7000;
        }
    }
}