using OELib.LibraryBase;

namespace OELib.PokingConnection
{
    public class PokingClientConnection : ReconnectingClientSideConnection, IPokingConnection
    {
        private object _reactingObject;

        public Reactor Reactor { get; private set; }

        public PokingClientConnection(object reactingObject)
        {
            _reactingObject = reactingObject;
            Reactor = new Reactor(this, _reactingObject);
        }

        public override bool Start(string IpAddress, int port)
        {
            var success = base.Start(IpAddress, port);
            if (success) Reactor.Start();
            return success;
        }
    }
}