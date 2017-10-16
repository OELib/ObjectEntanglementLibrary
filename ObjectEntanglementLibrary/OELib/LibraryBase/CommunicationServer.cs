using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace OELib.LibraryBase
{
    public class CommunicationServer<T> where T : Connection
    {
        private readonly TcpListener _listener;

        public CommunicationServer(IPEndPoint localEndPoint)
        {
            _listener = new TcpListener(localEndPoint);
        }

        private Actor connectionManager { get; } = new Actor();

        // ReSharper disable once InconsistentNaming
        protected List<T> _connections { get; } = new List<T>();

        public List<T> Connections
        {
            get
            {
                var connections = new List<T>();
                connectionManager.PostWait(() => { _connections.ForEach(c => connections.Add(c)); });
                return connections;
            }
        }

        public event EventHandler<T> ClientConnected;

        public event EventHandler<Tuple<T, Exception>> ClientDisconnected
            ; //todo: use special class, not the ugly tuple :(

        public void Start()
        {
            _listener.Start();
            _listener.BeginAcceptTcpClient(callback, _listener);
        }

        private void callback(IAsyncResult ar)
        {
            var lisetner = ar.AsyncState as TcpListener; // maybe stupid not to use _listener here?
            TcpClient client;
            try
            {
                // ReSharper disable once PossibleNullReferenceException
                client = lisetner.EndAcceptTcpClient(ar);
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            var connection = createInstance(client);

            connectionManager.PostWait(() =>
            {
                _connections.Add(connection);
                ClientConnected?.Invoke(this, connection); //, null, null);
            });

            connection.Stopped += (s, e) =>
            {
                connectionManager.PostWait(() => _connections.Remove(s as T));
                ClientDisconnected?.Invoke(this, new Tuple<T, Exception>(s as T, e)); //, null, null);
            };

            _listener.BeginAcceptTcpClient(callback, _listener);
        }

        protected virtual T createInstance(TcpClient client)
        {
            return Activator.CreateInstance(typeof(T), client) as T;
        }
    }
}