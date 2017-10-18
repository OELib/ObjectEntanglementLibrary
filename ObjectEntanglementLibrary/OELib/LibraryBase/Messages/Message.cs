using System;

namespace OELib.LibraryBase.Messages
{
    [Serializable]
    public abstract class Message
    {
        public Priority Priority { get; set; } = Priority.Normal;
    }
}