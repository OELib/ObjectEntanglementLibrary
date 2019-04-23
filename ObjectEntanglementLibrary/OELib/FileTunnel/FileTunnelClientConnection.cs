using System;
using System.Runtime.Serialization;
using OELib.LibraryBase;

namespace OELib.FileTunnel
{
    public class FileTunnelClientConnection : ReconnectingClientSideConnection, IFileTunnelConnection
    {
        public event EventHandler<MessageCarrier> MessageCarrierReceived; 

        public FileTunnelClientConnection(IFormatter customFormatter = null, ILogger logger = null, bool useCompression = false)
            : base(customFormatter, logger, useCompression)
        {
            MessageRecieved += FileTunnelClientConnection_MessageRecieved;
            Name = "File tunnel client connection.";
            PingInterval = 10000;
        }

        private void FileTunnelClientConnection_MessageRecieved(object sender, LibraryBase.Messages.Message e)
        {
            if (e is MessageCarrier mc)
                MessageCarrierReceived?.Invoke(this, mc);
        }

        public bool SendMessageCarrier<T>(T messageCarrier)
        {
            MessageCarrier mc = messageCarrier as MessageCarrier;
            return SendMessage(mc);
        }
    }
}
