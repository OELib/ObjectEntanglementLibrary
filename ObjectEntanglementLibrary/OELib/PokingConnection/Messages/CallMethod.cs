using OELib.LibraryBase.Messages;
using System;

namespace OELib.PokingConnection.Messages
{
    [Serializable]
    public class CallMethod : TraceableMessage
    {
        public CallMethod(string methodName, object[] arguments)
        {
            MethodName = methodName;
            Arguments = arguments;
        }

        public object[] Arguments { get; private set; }
        public string MethodName { get; private set; }
    }
}