using System.Runtime.Serialization;
using OELib.LibraryBase;

namespace OELib.FileExchange
{
    public class FileExchangeClientConnection : ReconnectingClientSideConnection, IFileConnection
    {

        public FileExchangeClientConnection(string rootDir, IFormatter customFormatter = null, ILogger logger = null,
            bool useCompression = false)
            : base(customFormatter, logger, useCompression)
        {
            FileManager = new FileExchangeManager(rootDir, this);
        }

        public FileExchangeManager FileManager { get; }

    }
}
