using System;
using System.Net;
using OELib.LibraryBase.Messages;
using OELib.LibraryBase;


namespace OELib.PokingConnection.ObjectTunnel //todo: change namespace
{
    public class ObjectTunnelServer
    {
        private readonly CommunicationServer<ServerSideConnection> _server;

        public ObjectTunnelServer(int port)
        {
            _server = new CommunicationServer<ServerSideConnection>(new IPEndPoint(IPAddress.Parse("127.0.0.1"), port));
            _server.Start();
            _server.ClientConnected += _server_ClientConnected;

        }


        private void _server_ClientConnected(object sender, ServerSideConnection e)
        {
            e.MessageRecieved += E_MessageRecieved;
        }

        private void E_MessageRecieved(object sender, Message e)
        {
            var oc = e as ObjectCarrier;
            ObjectReceived?.Invoke(this, oc == null ? e : oc.Payload);
        }

        public event EventHandler<object> ObjectReceived;


        public void SendObject<T>(T objectOut)
        {
            var m = objectOut as Message ?? new ObjectCarrier() { Payload = objectOut };
            _server.Connections.ForEach(c =>
                {
                    c.SendMessage(m);
                });
        }
    }

   
}