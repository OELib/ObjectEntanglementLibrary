using System;

namespace OELib.ObjectTunnel
{
    public interface IObjectTunnelConnection
    {
        event EventHandler<object> ObjectReceived;
        bool SendObject<T>(T objectToSend);
    }
}
