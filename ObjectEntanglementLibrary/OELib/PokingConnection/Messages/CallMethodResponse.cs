using OELib.LibraryBase.Messages;
using System;

namespace OELib.PokingConnection.Messages
{
    [Serializable]
    public class CallMethodResponse : TraceableMessage
    {
        public object Response { get; }
        public Exception Exception { get; }
        public CallMethodResponse(CallMethod callingMessage, object response, Exception exception)
            : base(callingMessage)
        {
            Response = response;
            Exception = exception;
        }        
    }
}
