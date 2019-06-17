using System;
using System.Collections.Generic;
using OELib.LibraryBase.Messages;

namespace OELib.FileExchange
{
    public abstract class FileInfoMessage : Message
    {
        public Guid Guid { get; } = new Guid();
    }

    public class FileInfoRequest : FileInfoMessage
    {
    }

    public class FileInfoResponse : FileInfoMessage
    {
        public Guid CallerGuid { get; protected set; }

        public FileInfoResponse(FileInfoRequest request)
        {
            CallerGuid = request.Guid;
        }
    }

    public class FileListingRequest : FileInfoRequest
    {
    }

    public class FileListingResponse : FileInfoResponse
    {
        public List<FileInformation> FileList { get; set; }
        public FileListingResponse(FileListingRequest request, List<FileInformation> fileList)
        : base(request)
        {
            FileList = fileList;
        }
    }

    public class FileGetRequest : FileInfoRequest
    {
        public FileInformation FileToGet { get; set; }
        public FileGetRequest(FileInformation fileToRetrieve)
        {
            FileToGet = fileToRetrieve;
        }
    }

    public class FileGetResponse : FileInfoResponse
    {
        public FileInformation FileToGet { get; set; }
        public byte[] Data { get; set; }
        public FileGetResponse(FileGetRequest request, byte[] data)
        : base(request)
        {
            FileToGet = request.FileToGet;
            Data = data;
        }
    }

}
