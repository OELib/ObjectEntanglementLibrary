using System;

namespace OELib.LibraryBase.Messages
{
    [Serializable]
    public class TraceableMessage : Message
    {
        public TraceableMessage()
        {
            MessageID = Guid.NewGuid();
        }

        public TraceableMessage(TraceableMessage callingMessage)
            : this()
        {
            CallingMessageID = callingMessage.MessageID;
        }

        public Guid MessageID { get; protected set; }
        public Guid CallingMessageID { get; protected set; }
    }
}