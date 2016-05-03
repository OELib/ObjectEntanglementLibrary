using OELib.LibraryBase.Messages;
using System;

namespace OELib.PokingConnection.Messages
{
    [Serializable]
    public class RemoteObjectInspectionResult : TraceableMessage
    {
        public ObjectInfo ObjectInfo { get; protected set; }

        public RemoteObjectInspectionResult(InspectRemoteObject msg, ObjectInfo objectInfo)
            : base(msg)
        {
            this.ObjectInfo = objectInfo;
        }

        public override string ToString()
        {
            return "Remote inspection result: " + ObjectInfo != null ? "Object info" : "NO OBJECT info";
        }
    }
}