using OELib.PokingConnection;
using System.Net.Sockets;

namespace OELib.ObjectEntanglement
{
    public class EntangledServerConnection<T> : PokingServerConnection
    {
        public T RemoteEntangledObject { get; }

        public EntangledServerConnection(TcpClient client, object localReactingObject)
            : base(client, localReactingObject)
        {
            RemoteEntangledObject = EntangeledObjectFactory.CreateEntangledObject<T>(
                this.GetType().Namespace,
                this.GetType().Module.Name,
                "EntangledObject",
                Reactor);
        }
    }
}