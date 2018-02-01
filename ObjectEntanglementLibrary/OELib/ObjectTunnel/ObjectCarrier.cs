using System;
using OELib.LibraryBase.Messages;

namespace OELib.ObjectTunnel
{
    [Serializable]
    public class ObjectCarrier : Message
    {
        public object Payload { get; set; }
    }
}
