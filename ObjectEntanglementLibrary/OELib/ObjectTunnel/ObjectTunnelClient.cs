using OELib.PokingConnection;
using System;
using System.Net;
using OELib.LibraryBase;
using OELib.LibraryBase.Messages;

namespace OELib.PokingConnection.ObjectTunnel //todo: change namespace
{
    public class ObjectTunnelClient
    {
        private readonly ReconnectingClientSideConnection _client;

        private readonly IPEndPoint _remoteEndpoint;

        public ObjectTunnelClient(IPEndPoint remoteEndpoint)
        {
            _remoteEndpoint = remoteEndpoint;
            _client = new ReconnectingClientSideConnection();
            _client.Started += (s, e) => { Connected?.Invoke(s, null); };
            _client.Stopped += (s, e) => { Disconnected?.Invoke(s, null); };
            _client.MessageRecieved += _client_MessageRecieved;
        }

        private void _client_MessageRecieved(object sender, Message e)
        {
            var oc = e as ObjectCarrier;
            ObjectReceived?.Invoke(this, oc == null ? e : oc.Payload);
        }

        public event EventHandler Connected;

        public event EventHandler Disconnected;

        public event EventHandler<object> ObjectReceived;

        

        public void SendObject<T>(T objectOut)
        {
            var m = objectOut as Message ?? new ObjectCarrier() { Payload = objectOut };
            _client.SendMessage(m);
        }

        public void StartConnectionAttempts()
        {
            _client.Start(_remoteEndpoint.Address.ToString(), _remoteEndpoint.Port);
        }
    }
}