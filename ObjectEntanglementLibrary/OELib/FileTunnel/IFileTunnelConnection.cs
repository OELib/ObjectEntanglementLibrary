using System;

namespace OELib.FileTunnel
{
    public interface IFileTunnelConnection
    {
        event EventHandler<MessageCarrier> MessageCarrierReceived;
        bool SendMessageCarrier<T>(T messageCarrier);
    }
}
