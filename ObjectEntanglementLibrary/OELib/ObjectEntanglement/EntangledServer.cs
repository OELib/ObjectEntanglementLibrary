using OELib.PokingConnection;
using System.Net.Sockets;

namespace OELib.ObjectEntanglement
{
    public class EntangledServer<T> : PokingServer
    {
        public EntangledServer(int port, object localReactingObject)
            : base(port, localReactingObject)
        {
        }

        protected override PokingServerConnection createInstance(TcpClient client) =>
            new EntangledServerConnection<T>(client, _reactingObject);
    }
}