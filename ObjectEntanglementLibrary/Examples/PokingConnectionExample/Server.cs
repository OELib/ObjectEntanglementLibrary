using OELib.PokingConnection;

namespace PokingConnectionExample
{
    public class Server
    {
        private readonly PokingServer _server;

        public Server()
        {
            _server = new PokingServer(1024, this);
            _server.Start();
        }


        public void SendPayload(Payload pl)
        {
            _server.Connections.ForEach(c =>
                c.Reactor.CallRemoteMethod("EchoPayload", pl));
        }
    }
}
