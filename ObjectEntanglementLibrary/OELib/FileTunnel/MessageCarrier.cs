using System;
using OELib.LibraryBase.Messages;

namespace OELib.FileTunnel
{
    public enum MessageType {   Object, FileNotFound,
                                FileRequest, FileContents,
                                ListFilesRequest, ListFilesResponse,
                                ListDirectoriesRequest, ListDirectoriesResponse}

    [Serializable]
    public class MessageCarrier : Message
    {
        public MessageCarrier(MessageType type)
        {
            Type = type;
        }

        public MessageType Type { get; set; }

        public object Payload { get; set; }
    }
}
