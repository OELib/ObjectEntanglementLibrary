using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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

        protected readonly ConcurrentDictionary<T, byte> _connections = new ConcurrentDictionary<T, byte>(); // byte is a dummy, it looks ugly but the concurrent dictionary is very efficient for this

        public List<T> Connections => _connections.ToArray().Select(kv => kv.Key).ToList();

        public event EventHandler<T> ClientConnected;

        public event EventHandler<Tuple<T, Exception>> ClientDisconnected; //todo: use special class, not the ugly tuple :(

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
            var listener = ar.AsyncState as TcpListener; // maybe stupid not to use _listener here?
            TcpClient client;
            try //TODO: don't try & catch, check and return.
            {
                // ReSharper disable once PossibleNullReferenceException
                client = listener.EndAcceptTcpClient(ar);
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            var connection = createInstance(client);
            var okAdded = _connections.TryAdd(connection, byte.MinValue);
            if (!okAdded) return; // key already exists
            _eventDriver.Post(() => ClientConnected?.Invoke(this, connection));
            connection.Stopped += (s, e) =>
            {
                var okRemoved = _connections.TryRemove((T)s, out _); // this will return false if the key was already removed
                if (okRemoved) _eventDriver.Post(() => ClientDisconnected?.Invoke(this, new Tuple<T, Exception>(s as T, e)));
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