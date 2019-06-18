using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using OELib.LibraryBase;

namespace OELib.FileExchange
{
    public class FileExchangeServer : CommunicationServer<FileExchangeServerConnection>
    {
        private readonly string _rootPath;

        public FileExchangeServer(string rootPath, IPEndPoint localEndPoint, IFormatter formatter = null, ILogger logger = null, bool useCompression = false)
            : base(localEndPoint, formatter, logger, useCompression)
        {
            _rootPath = rootPath;
        }

        protected override FileExchangeServerConnection createInstance(TcpClient client)
        {
            return new FileExchangeServerConnection(_rootPath, client, Formatter, Logger, UseCompression);
        }

    }
}
