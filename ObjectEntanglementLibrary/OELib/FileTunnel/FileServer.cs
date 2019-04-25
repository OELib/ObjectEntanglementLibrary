using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OELib.FileTunnel
{
    public class FileServer
    {
        public FileTunnelServer FileTunnelServer { get; set; }
        public FileRequestStack RequestStack { get; set; }

        public FileServer(string ip, int port)
        {
            FileTunnelServer = new FileTunnelServer(new IPEndPoint(IPAddress.Parse("127.0.0.1"), port));
            FileTunnelServer.Start();

            RequestStack = new FileRequestStack();

            FileTunnelServer.MessageCarrierReceived += OnServerMessageCarrierReceived;

            RequestStack.SendFile += OnServerSendFile;
            RequestStack.FileNotFound += OnServerFileNotFound;
        }

        public void OnServerMessageCarrierReceived(object sender, MessageCarrier mc)
        {
            if (mc.Type == MessageType.FileRequest)
                RequestStack.Push(sender as FileTunnelServerConnection, mc.Payload as string);

            if (mc.Type == MessageType.ListFilesRequest)
            {
                var ftsc = sender as FileTunnelServerConnection;
                var fileList = Directory.GetFiles(mc.Payload as string).ToList();
                ftsc.SendMessageCarrier(new MessageCarrier(MessageType.ListFilesResponse) { Payload = fileList });
            }
        }

        public void OnServerFileNotFound(object sender, FileRequestEventArgs e)
        {
            e.Connection.SendMessageCarrier(new MessageCarrier(MessageType.FileNotFound) { Payload = e.FilePathAndName });
        }

        public static void OnServerSendFile(object sender, FileRequestEventArgs e)
        {
            FileStream fs = new FileStream(e.FilePathAndName, FileMode.Open, FileAccess.Read);
            byte[] bytes = File.ReadAllBytes(e.FilePathAndName);
            fs.Read(bytes, 0, Convert.ToInt32(fs.Length));
            fs.Close();

            e.Connection.SendMessageCarrier(new MessageCarrier(MessageType.FileContents) { Payload = bytes });
        }
    }
}
