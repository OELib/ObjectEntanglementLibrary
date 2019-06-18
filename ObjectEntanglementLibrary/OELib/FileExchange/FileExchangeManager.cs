using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OELib.LibraryBase;

namespace OELib.FileExchange
{
    public class FileExchangeManager
    {
        private readonly string _rootDir;
        private readonly Connection _connection;

        public FileExchangeManager(string rootDir, Connection connection)
        {
            _rootDir = Path.GetFullPath(rootDir);
            if (!_rootDir.EndsWith("\\")) rootDir += "\\";
            hookEvents(connection);
            _connection = connection;
        }

        internal void hookEvents(Connection connection)
        {
            connection.MessageReceived += (conn, e) =>
            {
                if (e is IFileExchangeMessage msg)
                {
                    switch (msg)
                    {
                        case FileExchangeRequest req:
                            handleRequest(req, connection);
                            break;
                    }
                }
            };
        }

        private void handleRequest(FileExchangeRequest request, Connection client)
        {
            switch (request)
            {
                case FileListingRequest fir:
                    var fileListing = ListLocalFiles();
                    client.SendMessage(new FileListingResponse(fir, fileListing));
                    break;
                case FileGetRequest fgr:
                    handleFileGetRequest(fgr, client);
                    break;
            }
        }

        private void handleFileGetRequest(FileGetRequest fgr, Connection client)
        {
            var fgResp = new FileGetResponse(fgr, null);
            try
            {
                var fileListing = ListLocalFiles();
                var targetFile = fileListing.FirstOrDefault(f =>
                    f.FileName == fgr.FileToGet.FileName && f.Directory == fgr.FileToGet.Directory);
                if (targetFile == null) throw new FileNotFoundException("File does not exist on server");
                fgResp.Data = File.ReadAllBytes(Path.Combine(_rootDir, targetFile.Directory, targetFile.FileName));
            }
            catch (Exception ex)
            {
                fgResp.Exception = ex;
            }
            finally
            {
                client.SendMessage(fgResp);
            }
        }

        public List<FileInformation> ListLocalFiles()
        {
            //todo: make sure it traverses sub directories
            var fileNames = Directory.GetFiles(_rootDir).Select(fn=>Path.GetFullPath(fn).Substring(_rootDir.Length)).ToList();
            return fileNames.Select(fn => new FileInformation(_rootDir, fn)).ToList();
        }

        public FileListingResponse ListRemoteFiles()
        {
            return _connection.Ask(new FileListingRequest()) as FileListingResponse;
        }

        public FileInfo DownloadFile(FileInformation remoteFile)
        {
            var fileData = _connection.Ask(new FileGetRequest(remoteFile)) as FileGetResponse;
            if (fileData == null) throw new TimeoutException();
            if (fileData.Exception != null) throw fileData.Exception;
            var targetFileName = Path.Combine(_rootDir, fileData.FileToGet.Directory, fileData.FileToGet.FileName);
            File.WriteAllBytes(targetFileName, fileData.Data);
            return new FileInfo(targetFileName);
        }


    }
}
