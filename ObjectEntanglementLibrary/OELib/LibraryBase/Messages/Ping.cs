using System;

namespace OELib.LibraryBase.Messages
{
    [Serializable]
    public sealed class Ping : Message, IControlMessage
    {
    }
}