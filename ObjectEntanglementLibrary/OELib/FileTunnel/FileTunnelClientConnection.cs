using System;
using System.Runtime.Serialization;
using OELib.LibraryBase;


namespace OELib.FileTunnel
{
    public class FileTunnelClientConnection : ReconnectingClientSideConnection, IFileTunnelConnection
    {
        public event EventHandler<object> FileReceived; 

        public FileTunnelClientConnection(IFormatter customFormatter = null, ILogger logger = null, bool useCompression = false)
            : base(customFormatter, logger, useCompression)
        {
            MessageRecieved += FileTunnelClientConnection_MessageRecieved;
            Name = "Object tunnel client connection.";
            PingInterval = 10000;
        }

        private void FileTunnelClientConnection_MessageRecieved(object sender, LibraryBase.Messages.Message e)
        {
            if (e is FileCarrier carrier) FileReceived?.Invoke(this, carrier.Payload);
        }

        public bool SendFile<T>(T objectToSend)
        {
            var pl = new FileCarrier() {Payload = objectToSend};
            return SendMessage(pl);
        }
    }
}
