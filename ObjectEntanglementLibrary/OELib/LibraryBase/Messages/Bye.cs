using System;

namespace OELib.LibraryBase.Messages
{
    [Serializable]
    public class Bye : Message, IControlMessage
    {
        public Bye()
        {
            Priority = Priority.High;
        }
    }
}