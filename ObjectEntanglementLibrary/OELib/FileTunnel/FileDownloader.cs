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
        public bool LastReceiveSuccess { get; protected set; }

        private string _lastRequestedFile = "";

        ManualResetEvent mre = new ManualResetEvent(false);
        AutoResetEvent are = new AutoResetEvent(false);

        public FileDownloader(string ipAddress, int port, string downloadDirectory)
        {
            FileTunnelClient = new FileTunnelClientConnection();
            FileTunnelClient.Start(ipAddress, port);
            DownloadDirectory = downloadDirectory;          // eg @"C:\Users\ari\received\"
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
            mre.Reset();
            _lastRequestedFile = Path.GetFileName(remoteFilePathAndName);
            bool success = FileTunnelClient.SendMessageCarrier(new MessageCarrier(MessageType.FileRequest) { Payload = remoteFilePathAndName });
            mre.WaitOne();
            return (success && LastReceiveSuccess);
        }

        public void OnClientMessageCarrierReceived(object sender, MessageCarrier mc)
        {
            if (mc.Type == MessageType.FileNotFound)
            {
                LastReceiveSuccess = false;
            }

            if (mc.Type == MessageType.FileContents)
            {
                try
                {
                    File.WriteAllBytes(DownloadDirectory + _lastRequestedFile, mc.Payload as byte[]);
                    LastReceiveSuccess = true;
                }
                catch (Exception ex)
                {
                    LastReceiveSuccess = false;
                }
            }

            mre.Set();
        }
    }
}
