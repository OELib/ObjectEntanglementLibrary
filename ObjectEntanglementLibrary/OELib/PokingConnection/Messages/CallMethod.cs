using OELib.LibraryBase.Messages;
using System;

namespace OELib.PokingConnection.Messages
{
    [Serializable]
    public class CallMethod : TraceableMessage
    {
        public CallMethod(string methodName, object[] arguments, Type genericType)
        {
            MethodName = methodName;
            Arguments = arguments;
            GenericType = genericType;
        }

        public object[] Arguments { get; private set; }
        public string MethodName { get; private set; }
        public Type GenericType { get; private set; }

        public bool IsGenericMethod => GenericType != null;
    }
}