using System.Net.Sockets;
using System.Runtime.Serialization;
using OELib.LibraryBase;

namespace OELib.FileExchange
{
    public class FileExchangeServerConnection : ServerSideConnection, IFileConnection
    {
        public FileExchangeManager FileManager { get; }

        public FileExchangeServerConnection(FileExchangeManager manager, TcpClient client, IFormatter customFormatter = null, ILogger logger = null,
            bool useCompression = false)
            : base(client, customFormatter, logger, useCompression)
        {
            FileManager = manager;
            Start(client);
            FileManager.hookEvents(this);
        }
    }
}
