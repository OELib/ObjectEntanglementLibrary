﻿using System;
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
        public List<string> LastReceivedDirectoriesList { get; private set; }
        public FileProperties LastReceivedFileProperties { get; private set; }

        private string _lastRequestedFile = "";

        ManualResetEvent mreDownload = new ManualResetEvent(false);
        ManualResetEvent mreListFiles = new ManualResetEvent(false);
        ManualResetEvent mreListDirectories = new ManualResetEvent(false);
        ManualResetEvent mreListFileProperties = new ManualResetEvent(false);

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
            return LastReceiveFileSuccess;
        }

        /// <summary>
        /// Send a file list request. Server responds when it feels like it.
        /// </summary>
        public bool ListFilesRequest(string remotePath)
        {
            LastReceivedFileList = null;
            return FileTunnelClient.SendMessageCarrier(new MessageCarrier(MessageType.ListFilesRequest) { Payload = remotePath });
        }

        /// <summary>
        /// Send a file list request. Block until response has arrived.
        /// </summary>
        public bool ListFiles(string remotePath, out List<string> remoteFiles)
        {
            LastReceivedFileList = null;
            mreListFiles.Reset();
            bool sendSuccess = FileTunnelClient.SendMessageCarrier(new MessageCarrier(MessageType.ListFilesRequest) { Payload = remotePath });
            if (!sendSuccess) { remoteFiles = null; return false; }
            mreListFiles.WaitOne();
            remoteFiles = LastReceivedFileList;
            return sendSuccess;
        }

        /// <summary>
        /// Send a list directories request. Server responds when it feels like it.
        /// </summary>
        public bool ListDirectoriesRequest(string remotePath)
        {
            LastReceivedDirectoriesList = null;
            return FileTunnelClient.SendMessageCarrier(new MessageCarrier(MessageType.ListDirectoriesRequest) { Payload = remotePath });
        }

        /// <summary>
        /// Send a list directories request. Block until response has arrived.
        /// </summary>
        public bool ListDirectories(string remotePath, out List<string> remoteDirectories)
        {
            LastReceivedDirectoriesList = null;
            mreListDirectories.Reset();
            bool sendSuccess = FileTunnelClient.SendMessageCarrier(new MessageCarrier(MessageType.ListDirectoriesRequest) { Payload = remotePath });
            if (!sendSuccess) { remoteDirectories = null; return false; }
            mreListDirectories.WaitOne();
            remoteDirectories = LastReceivedDirectoriesList;
            return sendSuccess;
        }

        /// <summary>
        /// Send a get file properties request. Block until response has arrived.
        /// </summary>
        public bool GetFileProperties(string remoteFilePathAndName, out FileProperties properties)
        {
            LastReceivedFileProperties = null;
            mreListFileProperties.Reset();
            bool sendSuccess = FileTunnelClient.SendMessageCarrier(new MessageCarrier(MessageType.FilePropertiesRequest) { Payload = remoteFilePathAndName });
            if (!sendSuccess) { properties = null; return false; }
            mreListFileProperties.WaitOne();
            
            if (LastReceivedFileProperties != null)
            {
                properties = LastReceivedFileProperties;
                return true;
            }

            else
            {
                properties = null;
                return false;
            }
        }

        public void OnClientMessageCarrierReceived(object sender, MessageCarrier mc)
        {
            if (mc.Type == MessageType.FileNotFound)
            {
                LastReceiveFileSuccess = false;
                mreDownload.Set();
                mreListFiles.Set();
                mreListDirectories.Set();
                mreListFileProperties.Set();
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

            if (mc.Type == MessageType.ListDirectoriesResponse)
            {
                LastReceivedDirectoriesList = mc.Payload as List<string>;
                mreListDirectories.Set();
            }

            if (mc.Type == MessageType.FilePropertiesResponse)
            {
                LastReceivedFileProperties = mc.Payload as FileProperties;
                mreListFileProperties.Set();
            }
        }
    }
}
