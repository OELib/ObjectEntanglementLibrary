using System;
using System.Net.Sockets;
using System.Runtime.Serialization;
using OELib.FileExchange;
using OELib.LibraryBase;
using OELib.ObjectTunnel;
using OELib.PokingConnection;

namespace OELib.UniversalConnection
{
    public class UCServerConnection : ServerSideConnection, IPokingConnection, IObjectTunnelConnection
    {
        public FileExchangeManager FileManager { get; }
        public Reactor Reactor { get; }

        public UCServerConnection(TcpClient client, object reactingObject, string rootPath, IFormatter formatter = null, ILogger logger = null, bool useCompression = false)
            : base(formatter, logger, useCompression)
        {
            if (reactingObject != null)
                Reactor = new Reactor(this, reactingObject);
            MessageReceived += ObjectTunnelClientConnection_MessageReceived;
            if (Start(client))
            {
                Reactor?.Start();
                if (rootPath != null)
                    FileManager = new FileExchangeManager(rootPath, this);
            }
            else throw new Exception("Server connection failed.");
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
