using OELib.PokingConnection;


namespace PokingConnectionExample
{
    public class Client
    {
        private readonly PokingClientConnection _client;

        public Client()
        {
            _client = new PokingClientConnection(this);
            _client.Start("127.0.0.1", 1024);
        }


        public Payload EchoPayload(Payload pl)
        {
            return pl;
        }

    }
}
