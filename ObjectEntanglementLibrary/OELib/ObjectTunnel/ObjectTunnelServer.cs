using OELib.PokingConnection;
using System;

namespace OELib.PokingConnection.ObjectTunnel
{
    public class ObjectTunnelServer
    {
        private int _port;
        private PokingServer _server;

        public ObjectTunnelServer(int port)
        {
            _port = port;
            _server = new PokingServer(_port, this);
            _server.Start();
        }

        public event EventHandler<object> ObjectReceived;

        public void ReceiveObject(object objectIn)
        {
            if (ObjectReceived != null) ObjectReceived(this, objectIn);
        }

        public void SendObject<T>(T objectOut)
        {
            _server.Connections.ForEach(c =>
                {
                    c.Reactor.CallRemoteMethod("ReceiveObject", objectOut);
                });
        }
    }
}