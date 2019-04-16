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

        public event EventHandler<object> FileReceived;

        public FileTunnelServer(IPEndPoint localEndPoint, IFormatter formatter = null, ILogger logger = null, bool useCompression = false)
            : base(localEndPoint, formatter, logger, useCompression)
        {
        }

        protected override FileTunnelServerConnection createInstance(TcpClient client)
        {
            var r = new FileTunnelServerConnection(client, Formatter, Logger, UseCompression) { Name = "Server side connection", PingInterval=10000};
            r.FileReceived += R_FileRecieved;
            return r;
        }

        private void R_FileRecieved(object sender, object e)
        {
            FileReceived?.Invoke(sender, e);
        }

        public bool SendFile<T>(T fileToSend)
        {
            var results = new ConcurrentBag<bool>();
            Parallel.ForEach(Connections, c =>
            {
                if (c.IsReady) results.Add(c.SendFile(fileToSend));
            });
            return results.ToArray().All(r => r);
        }
    }
}
