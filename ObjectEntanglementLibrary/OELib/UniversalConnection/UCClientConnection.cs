using System;
using System.Runtime.Serialization;
using OELib.LibraryBase;
using OELib.ObjectTunnel;
using OELib.PokingConnection;

namespace OELib.UniversalConnection
{
    public class UCClientConnection : ReconnectingClientSideConnection, IPokingConnection, IObjectTunnelConnection
    {
        public Reactor Reactor { get; }

        public UCClientConnection(object reactingObject, IFormatter customFormatter = null, ILogger logger = null,
            bool useCompression = false)
            : base(customFormatter, logger, useCompression)
        {
            Reactor = new Reactor(this, reactingObject);
            MessageReceived += ObjectTunnelClientConnection_MessageReceived;
        }

        public override bool Start(string IpAddress, int port)
        {
            var success = base.Start(IpAddress, port);
            if (success) Reactor.Start();
            return success;
        }

        public event EventHandler<object> ObjectReceived;
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
