using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using OELib.LibraryBase;
using System.Collections.Concurrent;
using System.Linq;

namespace OELib.FileTunnel
{
    public class FileTunnelServer : CommunicationServer<FileTunnelServerConnection>, IFileTunnelConnection
    {
        public event EventHandler<MessageCarrier> MessageCarrierReceived;

        public FileTunnelServer(IPEndPoint localEndPoint, IFormatter formatter = null, ILogger logger = null, bool useCompression = false)
            : base(localEndPoint, formatter, logger, useCompression)
        {
        }

        protected override FileTunnelServerConnection createInstance(TcpClient client)
        {
            var r = new FileTunnelServerConnection(client, Formatter, Logger, UseCompression) { Name = "Server side connection", PingInterval = 10000};
            r.MessageCarrierReceived += R_ObjectRecieved;
            return r;
        }

        private void R_ObjectRecieved(object sender, MessageCarrier e)
        {
            MessageCarrierReceived?.Invoke(sender, e);
        }

        public bool SendBroadcastObject<T>(T objectToSend)
        {
            var results = new ConcurrentBag<bool>();
            var mc = new MessageCarrier(MessageType.Object) { Payload = objectToSend };

            Parallel.ForEach(Connections, c =>
            {
                if (c.IsReady)
                    results.Add(c.SendMessageCarrier(mc));
            });

            return results.ToArray().All(r => r);
        }

        public bool SendMessageCarrier<T>(T messageCarrier)
        {
            var results = new ConcurrentBag<bool>();

            Parallel.ForEach(Connections, c =>
            {
                if (c.IsReady)
                    results.Add(c.SendMessageCarrier(messageCarrier));
            });

            return results.ToArray().All(r => r);
        }
    }
}
