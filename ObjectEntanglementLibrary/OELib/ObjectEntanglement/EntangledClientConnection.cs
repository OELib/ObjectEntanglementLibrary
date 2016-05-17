using OELib.PokingConnection;

namespace OELib.ObjectEntanglement
{
    public class EntangledClientConnection<T> : PokingClientConnection
    {
        public EntangledClientConnection(object localReactingObject)
            : base(localReactingObject)
        {
            RemoteEntangledObject = EntangeledObjectFactory.CreateEntangledObject<T>(
                this.GetType().Namespace,
                this.GetType().Module.Name,
                "EntangledObject",
                Reactor);
        }

        public T RemoteEntangledObject { get; }
    }
}