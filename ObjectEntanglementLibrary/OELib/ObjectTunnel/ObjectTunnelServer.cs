using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using OELib.LibraryBase;
using System.Collections.Concurrent;
using System.Linq;

namespace OELib.ObjectTunnel
{
    public class ObjectTunnelServer : CommunicationServer<ObjectTunnelServerConnection>, IObjectTunnelConnection
    {

        public event EventHandler<object> ObjectReceived;

        public ObjectTunnelServer(IPEndPoint localEndPoint, IFormatter formatter = null, ILogger logger = null, bool useCompression = false)
            : base(localEndPoint, formatter, logger, useCompression)
        {
        }

        protected override ObjectTunnelServerConnection createInstance(TcpClient client)
        {
            var r = new ObjectTunnelServerConnection(client, Formatter, Logger, UseCompression) { Name = "Server side connection", PingInterval=10000};
            r.ObjectReceived += R_ObjectReceived;
            return r;
        }

        private void R_ObjectReceived(object sender, object e)
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
