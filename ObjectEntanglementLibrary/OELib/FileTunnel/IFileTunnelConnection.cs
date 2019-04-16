using System;

namespace OELib.FileTunnel
{
    public interface IFileTunnelConnection
    {
        event EventHandler<object> FileReceived;
        bool SendFile<T>(T fileToSend);
    }
}
