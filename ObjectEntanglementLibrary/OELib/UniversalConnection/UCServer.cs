using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using OELib.LibraryBase;
using OELib.ObjectTunnel;

namespace OELib.UniversalConnection
{
    public class UcServer : CommunicationServer<UCServerConnection>, IObjectTunnelConnection
    {

        private readonly object _reactingObject;
        private readonly string _rootPath;

        public UcServer(int port, object reactingObject, string rootPath, IFormatter formatter = null, ILogger logger = null, bool useCompression = false)
            : base(new IPEndPoint(IPAddress.Any, port), formatter, logger, useCompression)
        {
            _reactingObject = reactingObject;
            _rootPath = rootPath;
        }

        protected override UCServerConnection createInstance(TcpClient client)
        {
            var c = new UCServerConnection(client, _reactingObject, _rootPath, Formatter, Logger, UseCompression);
            c.ObjectReceived += C_ObjectReceived;
            return c;
        }

        public event EventHandler<object> ObjectReceived;

        private void C_ObjectReceived(object sender, object e)
        {
            ObjectReceived?.Invoke(sender, e);
        }

        public bool SendObject<T>(T objectToSend)
        {
            var results = new ConcurrentBag<bool>();
            Parallel.ForEach(Connections, c =>
            {
                if (c.IsReady) results.Add(c.SendObject(objectToSend));
            });
            return results.ToArray().All(r => r);
        }


    }
}
