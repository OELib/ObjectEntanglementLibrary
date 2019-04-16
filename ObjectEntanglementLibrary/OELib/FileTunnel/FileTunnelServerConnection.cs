using System;
using System.Net.Sockets;
using System.Runtime.Serialization;
using OELib.LibraryBase;

namespace OELib.FileTunnel
{
    public class FileTunnelServerConnection : ServerSideConnection, IFileTunnelConnection
    {
        public event EventHandler<object> FileReceived;

        public FileTunnelServerConnection(TcpClient client, IFormatter customFormatter = null, ILogger logger = null,
            bool useCompression = false)
            : base(client, customFormatter, logger, useCompression)
        {
            hookEvents();
        }
        
        protected void hookEvents()
        {
            MessageRecieved += FileTunnelClientConnection_MessageRecieved;
        }

        private void FileTunnelClientConnection_MessageRecieved(object sender, LibraryBase.Messages.Message e)
        {
            if (e is FileCarrier carrier) FileReceived?.Invoke(this, carrier.Payload);
        }

        public bool SendFile<T>(T fileToSend)
        {
            var pl = new FileCarrier() { Payload = fileToSend };
            return SendMessage(pl);
        }
    }
}