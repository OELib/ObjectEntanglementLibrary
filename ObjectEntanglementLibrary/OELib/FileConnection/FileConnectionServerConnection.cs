using System.Net.Sockets;
using System.Runtime.Serialization;
using OELib.LibraryBase;

namespace OELib.FileConnection
{
    public class FileConnectionServerConnection : ServerSideConnection, IFileConnection
    {
        public FileConnectionManager FileManager { get; }

        public FileConnectionServerConnection(FileConnectionManager manager, TcpClient client, IFormatter customFormatter = null, ILogger logger = null,
            bool useCompression = false)
            : base(client, customFormatter, logger, useCompression)
        {
            FileManager = manager;
            Start(client);
            MessageReceived += FileTunnelServerConnection_MessageReceived;
        }

        private void FileTunnelServerConnection_MessageReceived(object sender, LibraryBase.Messages.Message e)
        {
            if (!(e is FileInfoMessage msg)) return;
            FileManager.HandleFileInfoMessage(msg, this);
        }
    }
}
