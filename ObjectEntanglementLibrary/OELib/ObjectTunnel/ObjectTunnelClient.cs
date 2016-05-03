using OELib.PokingConnection;
using System;
using System.Net;

namespace OELib.LibraryBase.ObjectTunnel
{
    public class ObjectTunnelClient
    {
        private PokingClientConnection _client;

        private IPEndPoint _remoteEndpoint;

        public ObjectTunnelClient(IPEndPoint remoteEndpoint)
        {
            _remoteEndpoint = remoteEndpoint;
            _client = new PokingClientConnection(this);
            _client.Started += (s, e) => { Connected?.Invoke(s, null); };
            _client.Stopped += (s, e) => { Disconnected?.Invoke(s, null); };
        }

        public event EventHandler Connected;

        public event EventHandler Disconnected;

        public event EventHandler<object> ObjectReceived;

        public void ReceiveObject(object objectIn)
        {
            ObjectReceived?.Invoke(this, objectIn);
        }

        public void SendObject<T>(T objectOut)
        {
            _client.Reactor.CallRemoteMethod("ReceiveObject", objectOut);
        }

        public void StartConnectionAttempts()
        {
            _client.Start(_remoteEndpoint.Address.ToString(), _remoteEndpoint.Port);
        }
    }
}