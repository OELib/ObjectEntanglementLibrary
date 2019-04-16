using System;
using OELib.LibraryBase.Messages;

namespace OELib.FileTunnel
{
    [Serializable]
    public class FileCarrier : Message
    {
        public object Payload { get; set; }
    }
}
