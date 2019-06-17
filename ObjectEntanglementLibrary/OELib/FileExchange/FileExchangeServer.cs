using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using OELib.LibraryBase;

namespace OELib.FileExchange
{
    public class FileExchangeServer : CommunicationServer<FileExchangeServerConnection>, IFileConnection
    {
        public FileExchangeServer(string rootPath, IPEndPoint localEndPoint, IFormatter formatter = null, ILogger logger = null, bool useCompression = false)
            : base(localEndPoint, formatter, logger, useCompression)
        {
            ServerFileManager = new FileExchangeManager(rootPath);
        }

        public FileExchangeManager ServerFileManager { get; }


        protected override FileExchangeServerConnection createInstance(TcpClient client)
        {
            return new FileExchangeServerConnection(ServerFileManager, client, Formatter, Logger, UseCompression);
        }

    }
}
