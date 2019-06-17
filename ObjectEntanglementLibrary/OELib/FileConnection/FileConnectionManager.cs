using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OELib.LibraryBase;

namespace OELib.FileConnection
{
    public class FileConnectionManager
    {
        private readonly string _rootDir;

        public FileConnectionManager(string rootDir, Connection connection = null)
        {
            _rootDir = rootDir;
            if (connection != null)
                HookEvents(connection);

        }

        public void HookEvents(Connection connection)
        {
            connection.MessageReceived += (conn, e) =>
            {
                if (e is FileInfoMessage msg) HandleFileInfoMessage(msg, connection);
            };
        }

        //makes decision id the message is request or response
        public void HandleFileInfoMessage(FileInfoMessage msg, Connection client)
        {
            switch (msg)
            {
                case FileInfoRequest req:
                    handleRequest(req, client);
                    break;
                case FileInfoResponse resp:
                    handleResponse(resp, client);
                    break;
            }
        }

        public event EventHandler<List<FileInformation>> FileListingDone;

        private void handleResponse(FileInfoResponse resp, Connection client)
        {
            if (resp is FileListingResponse flr)
                FileListingDone?.Invoke(this, flr.FileList);
        }


        private void handleRequest(FileInfoRequest request, Connection client)
        {
            switch (request)
            {
                case FileListingRequest fir:
                    List<FileInformation> fileListing = createFilesList();
                    client.SendMessage(new FileListingResponse(fir, fileListing));
                    break;

                case FileGetRequest fgr:
                    break;
            }

        }

        private List<FileInformation> createFilesList()
        {
            var fileNames = Directory.GetFiles(_rootDir);
            return fileNames.Select(fn => new FileInformation(_rootDir, fn)).ToList();
        }
    }
}
