using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using OELib.LibraryBase;

namespace OELib.FileConnection
{
    public class FileConnectionServer : CommunicationServer<FileConnectionServerConnection>, IFileConnection
    {
        public FileConnectionServer(string rootPath, IPEndPoint localEndPoint, IFormatter formatter = null, ILogger logger = null, bool useCompression = false)
            : base(localEndPoint, formatter, logger, useCompression)
        {
            ServerFileManager = new FileConnectionManager(rootPath);
        }

        public FileConnectionManager ServerFileManager { get; }


        protected override FileConnectionServerConnection createInstance(TcpClient client)
        {
            return new FileConnectionServerConnection(ServerFileManager, client, Formatter, Logger, UseCompression);
        }

    }
}
