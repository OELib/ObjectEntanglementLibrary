using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OELib.FileTunnel
{
    public class FileDownloader
    {
        public FileTunnelClientConnection FileTunnelClient { get; set; }
        public string DownloadDirectory { get; set; }
        public bool LastReceiveFileSuccess { get; protected set; }
        public List<string> LastReceivedFileList { get; protected set; }

        private string _lastRequestedFile = "";

        ManualResetEvent mreDownload = new ManualResetEvent(false);
        ManualResetEvent mreListFiles = new ManualResetEvent(false);

        public FileDownloader(string ipAddress, int port, string downloadDirectory)
        {
            FileTunnelClient = new FileTunnelClientConnection();
            FileTunnelClient.Start(ipAddress, port);
            DownloadDirectory = downloadDirectory;
            FileTunnelClient.MessageCarrierReceived += OnClientMessageCarrierReceived;
        }

        /// <summary>
        /// Send a file download request. Server responds when it feels like it.
        /// </summary>
        /// <param name="remoteFilePathAndName"></param>
        /// <returns></returns>
        public bool DownloadRequest(string remoteFilePathAndName)
        {
            _lastRequestedFile = Path.GetFileName(remoteFilePathAndName);

            return FileTunnelClient.SendMessageCarrier(new MessageCarrier(MessageType.FileRequest) { Payload = remoteFilePathAndName });
        }

        /// <summary>
        /// Send a file download request. Block until file/response has arrived.
        /// </summary>
        /// <param name="remoteFilePathAndName"></param>
        /// <returns></returns>
        public bool Download(string remoteFilePathAndName)
        {
            mreDownload.Reset();
            _lastRequestedFile = Path.GetFileName(remoteFilePathAndName);
            bool sendSuccess = FileTunnelClient.SendMessageCarrier(new MessageCarrier(MessageType.FileRequest) { Payload = remoteFilePathAndName });
            mreDownload.WaitOne();
            return (sendSuccess && LastReceiveFileSuccess);
        }

        /// <summary>
        /// Send a file list request. Server responds when it feels like it.
        /// </summary>
        /// <param name="remotePath"></param>
        /// <returns></returns>
        public bool ListFilesRequest(string remotePath)
        {
            return FileTunnelClient.SendMessageCarrier(new MessageCarrier(MessageType.ListFilesRequest) { Payload = remotePath });
        }

        /// <summary>
        /// Send a file list request. Block until response has arrived.
        /// </summary>
        /// <param name="remotePath"></param>
        /// <param name="remoteFiles"></param>
        /// <returns></returns>
        public bool ListFiles(string remotePath, out List<string> remoteFiles)
        {
            mreListFiles.Reset();
            bool sendSuccess = FileTunnelClient.SendMessageCarrier(new MessageCarrier(MessageType.ListFilesRequest) { Payload = remotePath });
            mreListFiles.WaitOne();
            remoteFiles = LastReceivedFileList;
            return sendSuccess;
        }

        public void OnClientMessageCarrierReceived(object sender, MessageCarrier mc)
        {
            if (mc.Type == MessageType.FileNotFound)
            {
                LastReceiveFileSuccess = false;
                mreDownload.Set();
            }

            if (mc.Type == MessageType.FileContents)
            {
                try
                {
                    File.WriteAllBytes(DownloadDirectory + _lastRequestedFile, mc.Payload as byte[]);
                    LastReceiveFileSuccess = true;
                }
                catch (Exception ex)
                {
                    LastReceiveFileSuccess = false;
                }

                mreDownload.Set();
            }

            if (mc.Type == MessageType.ListFilesResponse)
            {
                LastReceivedFileList = mc.Payload as List<string>;
                mreListFiles.Set();
            }
        }
    }
}
