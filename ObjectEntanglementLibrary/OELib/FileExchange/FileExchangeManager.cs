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
                        case FileExchangeResponse resp:
                            handleResponse(resp);
                            break;
                    }
                }
            };
        }

        public event EventHandler<FileInformation> RemoteFileCreated;
        public event EventHandler<FileInformation> RemoteFileDeleted;
        public event EventHandler<FileInformation> RemoteFileModified;

        private readonly Actor _eventDriver = new Actor();

        private void handleResponse(FileExchangeResponse resp)
        {
            switch (resp)
            {
                case FileChangeNotification fcn:
                    switch (fcn.ChangeType)
                    {
                        case FileChangeNotification.FileChangeType.Created:
                            _eventDriver.Post(() => RemoteFileCreated?.Invoke(this, fcn.ModifiedFile));
                            break;
                        case FileChangeNotification.FileChangeType.Deleted:
                            _eventDriver.Post(() => RemoteFileDeleted?.Invoke(this, fcn.ModifiedFile));
                            break;
                        case FileChangeNotification.FileChangeType.Modified:
                            _eventDriver.Post(() => RemoteFileModified?.Invoke(this, fcn.ModifiedFile));
                            break;
                    }
                    break;

            }
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
                case FileTrackChangesRequest tcr:
                    handleTrackChangesRequest(tcr);
                    break;
            }
        }

        private FileTrackChangesRequest _trackChangesRequest = null;

        private void handleTrackChangesRequest(FileTrackChangesRequest tcr)
        {
            if (_trackChangesRequest == null) // first time
            {
                _trackChangesRequest = tcr;
                var fsv = new FileSystemWatcher(_rootDir);
                fsv.NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size;
                fsv.EnableRaisingEvents = true;
                fsv.Created += (s, fArg) =>
                {
                    _connection.SendMessage(new FileChangeNotification(_trackChangesRequest,
                        new FileInformation(_rootDir, fArg.Name), FileChangeNotification.FileChangeType.Created));
                };

                fsv.Deleted += (s, fArg) =>
                {
                    _connection.SendMessage(new FileChangeNotification(_trackChangesRequest,
                        new FileInformation(_rootDir, fArg.Name), FileChangeNotification.FileChangeType.Deleted));
                };

                fsv.Changed += (s, fArg) => // TODO: fix this, it does not work well. https://stackoverflow.com/questions/22447022/best-way-to-track-files-being-moved-possibly-between-disks-vb-net-or-c
                {
                    _connection.SendMessage(new FileChangeNotification(_trackChangesRequest,
                        new FileInformation(_rootDir, fArg.Name), FileChangeNotification.FileChangeType.Modified));
                };


            }

            _trackChangesRequest = tcr;
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
            var fileNames = Directory.GetFiles(_rootDir).Select(fn => Path.GetFullPath(fn).Substring(_rootDir.Length)).ToList();
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

        public void MonitorRemoteDirectoryChange()
        {
            _connection.SendMessage(new FileTrackChangesRequest());
        }

        public void ManuallyTriggerFileChange(FileInformation changedFile,
            FileChangeNotification.FileChangeType changeType)
        {
            var _ = new FileTrackChangesRequest(); // manually triggered change notification does not have a calling message.
            _connection.SendMessage(new FileChangeNotification(_, changedFile, changeType));
        }
    }
}
