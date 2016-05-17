using OELib.LibraryBase;
using System.Net;
using System.Net.Sockets;

namespace OELib.PokingConnection
{
    public class PokingServer : CommunicationServer<PokingServerConnection>
    {
        protected object _reactingObject { get; }

        public PokingServer(int port, object reactingObject)
            : base(new IPEndPoint(IPAddress.Any, port))
        {
            _reactingObject = reactingObject;
        }

        protected override PokingServerConnection createInstance(TcpClient client) => new PokingServerConnection(client, _reactingObject);
    }
}