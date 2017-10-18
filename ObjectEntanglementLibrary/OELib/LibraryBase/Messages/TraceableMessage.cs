using System;

namespace OELib.LibraryBase.Messages
{
    [Serializable]
    public abstract class TraceableMessage : Message
    {
        protected TraceableMessage()
        {
            MessageID = Guid.NewGuid();
        }

        protected TraceableMessage(TraceableMessage callingMessage)
            : this()
        {
            CallingMessageID = callingMessage.MessageID;
        }

        public Guid MessageID { get; set; }
        public Guid CallingMessageID { get; set; }
    }
}