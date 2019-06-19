using System.Net.Sockets;
using System.Runtime.Serialization;
using OELib.LibraryBase;

namespace OELib.FileExchange
{
    public class FileExchangeServerConnection : ServerSideConnection
    {
        public FileExchangeManager FileManager { get; }

        public FileExchangeServerConnection(string rootDir, TcpClient client, IFormatter customFormatter = null, ILogger logger = null,
            bool useCompression = false)
            : base(client, customFormatter, logger, useCompression)
        {
            Start(client);
            FileManager = new FileExchangeManager(rootDir, this);
        }
    }
}
