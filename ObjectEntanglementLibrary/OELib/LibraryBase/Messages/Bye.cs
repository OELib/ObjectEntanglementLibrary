using System;

namespace OELib.LibraryBase.Messages
{
    [Serializable]
    public sealed class Bye : Message, IControlMessage
    {
        public Bye()
        {
            Priority = Priority.High;
        }
    }
}