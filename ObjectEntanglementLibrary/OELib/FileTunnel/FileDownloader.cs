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

        private bool _lastResponseFileNotFound;
        private bool _lastReceiveFileSuccess;
        private List<string> _lastReceivedFileList;
        private List<string> _lastReceivedDirectoriesList;
        private FileProperties _lastReceivedFileProperties;

        private Action<string> WatcherFileModified;

        private string _lastRequestedFile = "";

        private ManualResetEvent mreDownload = new ManualResetEvent(false);
        private ManualResetEvent mreListFiles = new ManualResetEvent(false);
        private ManualResetEvent mreListDirectories = new ManualResetEvent(false);
        private ManualResetEvent mreListFileProperties = new ManualResetEvent(false);

        public FileDownloader(string ipAddress, int port, string downloadDirectory)
        {
            FileTunnelClient = new FileTunnelClientConnection();
            FileTunnelClient.Start(ipAddress, port);
            DownloadDirectory = downloadDirectory;
            FileTunnelClient.MessageCarrierReceived += OnClientMessageCarrierReceived;

            _lastResponseFileNotFound = false;
        }

        /// <summary>
        /// Send a file download request. Server responds when it feels like it.
        /// </summary>
        public bool DownloadRequest(string remoteFilePathAndName)
        {
            _lastRequestedFile = Path.GetFileName(remoteFilePathAndName);

            return FileTunnelClient.SendMessageCarrier(new MessageCarrier(MessageType.FileRequest) { Payload = remoteFilePathAndName });
        }

        /// <summary>
        /// Send a file download request. Block until file/response has arrived.
        /// </summary>
        public bool Download(string remoteFilePathAndName)
        {
            mreDownload.Reset();
            _lastRequestedFile = Path.GetFileName(remoteFilePathAndName);
            bool sendSuccess = FileTunnelClient.SendMessageCarrier(new MessageCarrier(MessageType.FileRequest) { Payload = remoteFilePathAndName });
            if (!sendSuccess) return false;
            mreDownload.WaitOne();
            return _lastReceiveFileSuccess;
        }

        /// <summary>
        /// Send a file list request. Server responds when it feels like it.
        /// </summary>
        public bool ListFilesRequest(string remotePath)
        {
            _lastReceivedFileList = null;
            return FileTunnelClient.SendMessageCarrier(new MessageCarrier(MessageType.ListFilesRequest) { Payload = remotePath });
        }

        /// <summary>
        /// Send a file list request. Block until response has arrived.
        /// </summary>
        public bool ListFiles(string remotePath, out List<string> remoteFiles)
        {
            _lastReceivedFileList = null;
            mreListFiles.Reset();
            bool sendSuccess = FileTunnelClient.SendMessageCarrier(new MessageCarrier(MessageType.ListFilesRequest) { Payload = remotePath });
            if (!sendSuccess) { remoteFiles = null; return false; }
            mreListFiles.WaitOne();
            remoteFiles = _lastReceivedFileList;
            return sendSuccess;
        }

        /// <summary>
        /// Send a list directories request. Server responds when it feels like it.
        /// </summary>
        public bool ListDirectoriesRequest(string remotePath)
        {
            _lastReceivedDirectoriesList = null;
            return FileTunnelClient.SendMessageCarrier(new MessageCarrier(MessageType.ListDirectoriesRequest) { Payload = remotePath });
        }

        /// <summary>
        /// Send a list directories request. Block until response has arrived.
        /// </summary>
        public bool ListDirectories(string remotePath, out List<string> remoteDirectories)
        {
            _lastReceivedDirectoriesList = null;
            mreListDirectories.Reset();
            bool sendSuccess = FileTunnelClient.SendMessageCarrier(new MessageCarrier(MessageType.ListDirectoriesRequest) { Payload = remotePath });
            if (!sendSuccess) { remoteDirectories = null; return false; }
            mreListDirectories.WaitOne();
            remoteDirectories = _lastReceivedDirectoriesList;
            return sendSuccess;
        }

        /// <summary>
        /// Send a get file properties request. Block until response has arrived.
        /// </summary>
        public bool GetFileProperties(string remoteFilePathAndName, out FileProperties properties)
        {
            _lastReceivedFileProperties = null;
            mreListFileProperties.Reset();
            bool sendSuccess = FileTunnelClient.SendMessageCarrier(new MessageCarrier(MessageType.FilePropertiesRequest) { Payload = remoteFilePathAndName });
            if (!sendSuccess) { properties = null; return false; }
            mreListFileProperties.WaitOne();
            
            if (_lastReceivedFileProperties != null)
            {
                properties = _lastReceivedFileProperties;
                return true;
            }

            else
            {
                properties = null;
                return false;
            }
        }

        public bool WatchDirectory(Action<string> modified)
        {
            bool sendSuccess = FileTunnelClient.SendMessageCarrier(new MessageCarrier(MessageType.WatchDirectoryRequest));

            if (sendSuccess)
            {
                WatcherFileModified = modified;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void OnClientMessageCarrierReceived(object sender, MessageCarrier mc)
        {            
            if (mc.Type == MessageType.FileNotFound)
            {
                _lastResponseFileNotFound = true;
                _lastReceiveFileSuccess = false;
                mreDownload.Set();
                mreListFiles.Set();
                mreListDirectories.Set();
                mreListFileProperties.Set();
            }

            else
            {
                _lastResponseFileNotFound = false;
            }

            if (mc.Type == MessageType.FileContents)
            {
                try
                {
                    File.WriteAllBytes(DownloadDirectory + _lastRequestedFile, mc.Payload as byte[]);
                    _lastReceiveFileSuccess = true;
                }
                catch (Exception ex)
                {
                    _lastReceiveFileSuccess = false;
                }

                mreDownload.Set();
            }

            if (mc.Type == MessageType.ListFilesResponse)
            {
                _lastReceivedFileList = mc.Payload as List<string>;
                mreListFiles.Set();
            }

            if (mc.Type == MessageType.ListDirectoriesResponse)
            {
                _lastReceivedDirectoriesList = mc.Payload as List<string>;
                mreListDirectories.Set();
            }

            if (mc.Type == MessageType.FilePropertiesResponse)
            {
                _lastReceivedFileProperties = mc.Payload as FileProperties;
                mreListFileProperties.Set();
            }

            if (mc.Type == MessageType.WatcherFileModified)
            {
                WatcherFileModified.Invoke(mc.Payload as string);
            }
        }
    }
}
