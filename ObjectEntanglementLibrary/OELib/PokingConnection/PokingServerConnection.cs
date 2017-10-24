using OELib.LibraryBase;
using System;
using System.Net.Sockets;
using System.Runtime.Serialization;

namespace OELib.PokingConnection
{
    public class PokingServerConnection : ServerSideConnection, IPokingConnection
    {
        public Reactor Reactor { get; protected set; }

        public PokingServerConnection(TcpClient client, object reactingObject, IFormatter formatter = null, ILogger logger = null, bool useCompression = false)
            : base(formatter, logger, useCompression)
        {
            Reactor = new Reactor(this, reactingObject);
            if (Start(client)) Reactor.Start();
            else throw new Exception("Server connection failed.");
        }
    }
}