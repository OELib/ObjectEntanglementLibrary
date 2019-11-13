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

        public IFormatter Formatter { get; set; }
        public ILogger Logger { get; set; }
        public bool UseCompression { get; set; }

        public CommunicationServer(IPEndPoint localEndPoint, IFormatter formatter = null, ILogger logger = null, bool useCompression = false)
        {

            Formatter = formatter;
            Logger = logger;
            UseCompression = useCompression;
            _listener = new TcpListener(localEndPoint);
        }


        private readonly Actor _eventDriver = new Actor();
        private readonly Actor _connectionManager = new Actor();

        // ReSharper disable once InconsistentNaming
        protected List<T> _connections { get; } = new List<T>(); //todo: replace this and the actor with a concurrent collection.

        public List<T> Connections
        {
            get
            {
                var connections = new List<T>();
                _connectionManager.PostWait(() => { _connections.ForEach(c => connections.Add(c)); });
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

            _connectionManager.PostWait(() =>
            {
                _connections.Add(connection);
                _eventDriver.Post(()=> ClientConnected?.Invoke(this, connection));
            });

            connection.Stopped += (s, e) =>
            {
                _connectionManager.PostWait(() => _connections.Remove(s as T));
                _eventDriver.Post(() => ClientDisconnected?.Invoke(this, new Tuple<T, Exception>(s as T, e)));
            };
            try
            {
                _listener.BeginAcceptTcpClient(callback, _listener);
            }
            catch (InvalidOperationException) { } // server stopped
            

        }

        protected virtual T createInstance(TcpClient client)
        {
            var r = Activator.CreateInstance(typeof(T), new object[] { client, Formatter, Logger, UseCompression }) as T; //TODO: there is nothing to guarantee that the constructor accepts this set of arguments. This should be fixed by calling an init method (with all args that are now in ctor) and a constraint on T.
            return r;
        }
    }
}