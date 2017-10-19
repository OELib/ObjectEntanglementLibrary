using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;

namespace OELib.LibraryBase
{
    public class CommunicationServer<T> where T : Connection
    {
        private readonly TcpListener _listener;

        public IFormatter Formatter { get; set; } = null;

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

        public void Stop() //todo: check if this really works.
        {
            _listener.Stop();
        }

        private void callback(IAsyncResult ar)
        {
            var lisetner = ar.AsyncState as TcpListener; // maybe stupid not to use _listener here?
            TcpClient client;
            try //TODO: don't try & catch, check and return.
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
            try
            {
                _listener.BeginAcceptTcpClient(callback, _listener);
            }
            catch (InvalidOperationException) { } // server stopped
            

        }

        protected virtual T createInstance(TcpClient client)
        {
            var r = Activator.CreateInstance(typeof(T), new object[] { client, null, null, false }) as T; //TODO: there is nothing to guarantee that the constructor accepts this set of arguments. This should be fixed by calling an init method (with all args that are now in ctor) and a constraint on T.
            if (r != null) r.Formatter = Formatter ?? r.Formatter; //todo: move formatter to init
            return r;
        }
    }
}