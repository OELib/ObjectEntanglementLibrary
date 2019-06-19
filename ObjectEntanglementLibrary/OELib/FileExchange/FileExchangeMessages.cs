using System;
using System.Collections.Generic;
using OELib.LibraryBase.Messages;

namespace OELib.FileExchange
{
    
    public interface IFileExchangeMessage
    {
    }

    [Serializable]
    public abstract class FileExchangeRequest : TraceableMessage, IFileExchangeMessage
    {
    }

    [Serializable]
    public abstract class FileExchangeResponse : TraceableMessage, IFileExchangeMessage
    {
        protected FileExchangeResponse(FileExchangeRequest request)
        :base(request)
        {
        }
    }

    [Serializable]
    public class FileListingRequest : FileExchangeRequest
    {
    }

    [Serializable]
    public class FileListingResponse : FileExchangeResponse
    {
        public List<FileInformation> FileList { get; }
        public FileListingResponse(FileListingRequest request, List<FileInformation> fileList)
        : base(request)
        {
            FileList = fileList;
        }
    }

    [Serializable]
    public class FileGetRequest : FileExchangeRequest
    {
        public FileInformation FileToGet { get; }
        public FileGetRequest(FileInformation fileToRetrieve)
        {
            FileToGet = fileToRetrieve;
        }
    }

    [Serializable]
    public class FileGetResponse : FileExchangeResponse
    {
        public FileInformation FileToGet { get; }
        public byte[] Data { get; set; }
        public Exception Exception { get; set; }

        public FileGetResponse(FileGetRequest request, byte[] data)
        : base(request)
        {
            FileToGet = request.FileToGet;
            Data = data;
        }
    }

    [Serializable]
    public class FileTrackChangesRequest : FileExchangeRequest
    {
    }

    [Serializable]
    public class FileChangeNotification : FileExchangeResponse
    {
        public enum FileChangeType { Created, Deleted, Modified}
        public FileChangeNotification(FileTrackChangesRequest request, FileInformation file, FileChangeType changeType)
            : base(request)
        {
            ModifiedFile = file;
            ChangeType = changeType;
        }

        public FileChangeType ChangeType { get; set; }

        public FileInformation ModifiedFile { get; set; }
    }

}
