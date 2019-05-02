using System;
using System.Net.Sockets;
using System.Runtime.Serialization;
using OELib.LibraryBase;

namespace OELib.FileTunnel
{
    public class FileTunnelServerConnection : ServerSideConnection, IFileTunnelConnection
    {
        public event EventHandler<MessageCarrier> MessageCarrierReceived;

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
            MessageCarrierReceived?.Invoke(this, e as MessageCarrier);
        }

        public bool SendMessageCarrier<T>(T messageCarrier)
        {
            MessageCarrier mc = messageCarrier as MessageCarrier;
            return SendMessage(mc);
        }
    }
}