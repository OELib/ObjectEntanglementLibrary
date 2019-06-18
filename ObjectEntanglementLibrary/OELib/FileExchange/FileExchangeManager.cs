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

        public FileExchangeManager(string rootDir, Connection connection = null)
        {
            _rootDir = rootDir;
            if (connection != null)
                hookEvents(connection);
        }

        internal void hookEvents(Connection connection)
        {
            connection.MessageReceived += (conn, e) =>
            {
                if (e is FileInfoMessage msg) handleFileInfoMessage(msg, connection);
            };
        }

        //makes decision id the message is request or response
        private void handleFileInfoMessage(FileInfoMessage msg, Connection client)
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
                FileListingDone?.Invoke(client, flr.FileList);
        }


        private void handleRequest(FileInfoRequest request, Connection client)
        {
            switch (request)
            {
                case FileListingRequest fir:
                    List<FileInformation> fileListing = ListLocalFiles();
                    client.SendMessage(new FileListingResponse(fir, fileListing));
                    break;

                case FileGetRequest fgr:
                    break;
            }
        }

        public List<FileInformation> ListLocalFiles()
        {
            var fileNames = Directory.GetFiles(_rootDir);
            return fileNames.Select(fn => new FileInformation(_rootDir, fn)).ToList();
        }




    }
}
