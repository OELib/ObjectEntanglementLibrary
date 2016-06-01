using System;

namespace OELib.LibraryBase.Messages
{
    [Serializable]
    public class Message
    {
        public Priority Priority { get; set; } = Priority.Normal;
    }
}