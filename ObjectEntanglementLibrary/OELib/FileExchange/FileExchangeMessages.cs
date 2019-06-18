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
        public List<FileInformation> FileList { get; set; }
        public FileListingResponse(FileListingRequest request, List<FileInformation> fileList)
        : base(request)
        {
            FileList = fileList;
        }
    }

    [Serializable]
    public class FileGetRequest : FileExchangeRequest
    {
        public FileInformation FileToGet { get; set; }
        public FileGetRequest(FileInformation fileToRetrieve)
        {
            FileToGet = fileToRetrieve;
        }
    }

    [Serializable]
    public class FileGetResponse : FileExchangeResponse
    {
        public FileInformation FileToGet { get; set; }
        public byte[] Data { get; set; }
        public Exception Exception { get; set; }
        public FileGetResponse(FileGetRequest request, byte[] data)
        : base(request)
        {
            FileToGet = request.FileToGet;
            Data = data;
        }
    }

}
