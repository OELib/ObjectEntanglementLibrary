using System;
using System.Runtime.Serialization;
using OELib.FileExchange;
using OELib.LibraryBase;
using OELib.ObjectTunnel;
using OELib.PokingConnection;

namespace OELib.UniversalConnection
{
    public class UCClientConnection : ReconnectingClientSideConnection, IPokingConnection, IObjectTunnelConnection
    {
        public Reactor Reactor { get; }
        public FileExchangeManager FileManager { get; }

        public UCClientConnection(object reactingObject, string rootPath, IFormatter customFormatter = null, ILogger logger = null,
            bool useCompression = false)
            : base(customFormatter, logger, useCompression)
        {
            if (reactingObject != null)
                Reactor = new Reactor(this, reactingObject);
            if (rootPath != null)
                FileManager = new FileExchangeManager(rootPath, this);
            MessageReceived += ObjectTunnelClientConnection_MessageReceived;
            this.Started += (_, __) =>
                Reactor.Start();
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
