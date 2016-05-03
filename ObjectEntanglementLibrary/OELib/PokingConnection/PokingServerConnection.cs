using OELib.LibraryBase;
using System;
using System.Net.Sockets;

namespace OELib.PokingConnection
{
    public class PokingServerConnection : ServerSideConnection, IPokingConnection
    {
        public Reactor Reactor { get; protected set; }

        public PokingServerConnection(TcpClient client, object reactingObject)
            : base()
        {
            Reactor = new Reactor(this, reactingObject);
            if (Start(client)) Reactor.Start();
            else throw new Exception("Server connection failed.");
        }
    }
}