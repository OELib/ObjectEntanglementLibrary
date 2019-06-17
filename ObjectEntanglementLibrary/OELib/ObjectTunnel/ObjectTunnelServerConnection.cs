using System;
using System.Net.Sockets;
using System.Runtime.Serialization;
using OELib.LibraryBase;

namespace OELib.ObjectTunnel
{
    public class ObjectTunnelServerConnection : ServerSideConnection, IObjectTunnelConnection
    {
        public event EventHandler<object> ObjectReceived;

        public ObjectTunnelServerConnection(TcpClient client, IFormatter customFormatter = null, ILogger logger = null,
            bool useCompression = false)
            : base(client, customFormatter, logger, useCompression)
        {
            hookEvents();
        }
        
        protected void hookEvents()
        {
            MessageReceived += ObjectTunnelClientConnection_MessageReceived;
        }

        private void ObjectTunnelClientConnection_MessageReceived(object sender, LibraryBase.Messages.Message e)
        {
            if (e is ObjectCarrier carrier) ObjectReceived?.Invoke(this, carrier.Payload);
        }

        public bool SendObject<T>(T objectToSend)
        {
            var pl = new ObjectCarrier() { Payload = objectToSend };
            return SendMessage(pl);
        }
    }
}