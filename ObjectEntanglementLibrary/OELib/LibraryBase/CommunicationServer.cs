using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace OELib.LibraryBase
{
    public class CommunicationServer<T> where T : Connection
    {
        public event EventHandler<T> ClientConnected;

        public event EventHandler<Tuple<T, Exception>> ClientDisconnected; //todo: use special class, not the ugly tuple :(

        private TcpListener _listener;

        private Actor _connectionManager { get; } = new Actor();

        protected List<T> _connections { get; } = new List<T>();

        public List<T> Connections
        {
            get
            {
                List<T> connections = new List<T>();
                _connectionManager.PostWait(() =>
                {
                    _connections.ForEach(c => connections.Add(c));
                });
                return connections;
            }
        }

        public CommunicationServer(IPEndPoint localEndPoint)
        {
            _listener = new TcpListener(localEndPoint);
        }

        public void Start()
        {
            _listener.Start();
            _listener.BeginAcceptTcpClient(callback, _listener);
        }

        private void callback(IAsyncResult ar)
        {
            var lisetner = (ar.AsyncState as TcpListener); // maybe stupid not to use _listener here?
            TcpClient client = null;
            try
            {
                client = lisetner.EndAcceptTcpClient(ar);
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            T connection = createInstance(client);

            _connectionManager.PostWait(() =>
            {
                _connections.Add(connection);
                ClientConnected?.Invoke(this, connection);//, null, null);
            });

            connection.Stopped += (s, e) =>
            {
                _connectionManager.PostWait(() => _connections.Remove(s as T));
                ClientDisconnected?.Invoke(this, new Tuple<T, Exception>(s as T, e));//, null, null);
            };

            _listener.BeginAcceptTcpClient(callback, _listener);
        }

        protected virtual T createInstance(TcpClient client) => Activator.CreateInstance(typeof(T), new object[] { client }) as T;
    }
}
