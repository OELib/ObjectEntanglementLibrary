using System;
using OELib.LibraryBase.Messages;

namespace OELib.PokingConnection.ObjectTunnel //todo: change namespace
{
    [Serializable]
    public class ObjectCarrier : Message
    {
        public object Payload { get; set; }
    }
}
