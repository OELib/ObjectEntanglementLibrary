using System;
using OELib.LibraryBase.Messages;

namespace OELib.FileTunnel
{
    public enum MessageType {Object, FileRequest, FileNotFound, FileContents, AvailableFilesRequest, AvailableFilesResponse }

    [Serializable]
    public class MessageCarrier : Message
    {
        public MessageCarrier(MessageType type)
        {
            Type = type;
        }

        public MessageType Type { get; set; }

        public object Payload { get; set; }     // byte array of file
    }
}
