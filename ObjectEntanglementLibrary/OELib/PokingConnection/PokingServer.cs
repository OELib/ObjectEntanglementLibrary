using OELib.LibraryBase;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;

namespace OELib.PokingConnection
{
    public class PokingServer : CommunicationServer<PokingServerConnection>
    {
        private object reactingObject { get; }

        public PokingServer(int port, object reactingObject, IFormatter formatter = null, ILogger logger = null, bool useCompression = false)
            : base(new IPEndPoint(IPAddress.Any, port), formatter, logger, useCompression)
        {
            this.reactingObject = reactingObject;
        }

        protected override PokingServerConnection createInstance(TcpClient client)
        {
            var c = new PokingServerConnection(client, reactingObject, Formatter, Logger, UseCompression);
            return c;
        }
    }
}