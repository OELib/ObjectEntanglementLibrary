using System.Runtime.Serialization;
using OELib.LibraryBase;

namespace OELib.PokingConnection
{
    public class PokingClientConnection : ReconnectingClientSideConnection, IPokingConnection
    {
        public Reactor Reactor { get; }

        public PokingClientConnection(object reactingObject, IFormatter customFormatter = null, ILogger logger = null, bool useCompression = false)
            : base(customFormatter, logger, useCompression)
        {
            Reactor = new Reactor(this, reactingObject);
        }

        public override bool Start(string IpAddress, int port)
        {
            var success = base.Start(IpAddress, port);
            if (success) Reactor.Start();
            return success;
        }
    }
}