using System;

namespace OELib.LibraryBase.Messages
{
    [Serializable]
    public sealed class Pong : Message, IControlMessage
    {
    }
}