using System;
using System.Runtime.Serialization;
using OELib.LibraryBase;


namespace OELib.ObjectTunnel
{
    public class ObjectTunnelClientConnection : ReconnectingClientSideConnection, IObjectTunnelConnection
    {
        public event EventHandler<object> ObjectReceived; 

        public ObjectTunnelClientConnection(IFormatter customFormatter = null, ILogger logger = null, bool useCompression = false)
            : base(customFormatter, logger, useCompression)
        {
            MessageRecieved += ObjectTunnelClientConnection_MessageReceived;
            Name = "Object tunnel client connection.";
            PingInterval = 10000;
        }

        private void ObjectTunnelClientConnection_MessageReceived(object sender, LibraryBase.Messages.Message e)
        {
            if (e is ObjectCarrier carrier) ObjectReceived?.Invoke(this, carrier.Payload);
        }

        public bool SendObject<T>(T objectToSend)
        {
            var pl = new ObjectCarrier() {Payload = objectToSend};
            return SendMessage(pl);
        }
    }
}
