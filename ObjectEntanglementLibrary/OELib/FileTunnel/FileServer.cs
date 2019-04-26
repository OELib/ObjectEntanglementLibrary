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
        public string RootDirectory { get; set; }

        public FileServer(string ip, int port, string rootDirectory)
        {
            FileTunnelServer = new FileTunnelServer(new IPEndPoint(IPAddress.Parse("127.0.0.1"), port));
            FileTunnelServer.Start();

            RequestStack = new FileRequestStack();
            RootDirectory = rootDirectory;

            FileTunnelServer.MessageCarrierReceived += OnServerMessageCarrierReceived;
            RequestStack.SendFile += OnServerSendFile;
            RequestStack.FileNotFound += OnServerFileNotFound;
        }

        public void OnServerMessageCarrierReceived(object sender, MessageCarrier mc)
        {
            if (mc.Type == MessageType.FileRequest)
                RequestStack.Push(sender as FileTunnelServerConnection, AddPaths(RootDirectory, mc.Payload as string));

            if (mc.Type == MessageType.ListFilesRequest)
            {
                var ftsc = sender as FileTunnelServerConnection;

                try
                {
                    // Run this just to throw exception if path doesn't exist
                    Path.GetFullPath(AddPaths(RootDirectory, mc.Payload as string));

                    var fileList = Directory.GetFiles(AddPaths(RootDirectory, mc.Payload as string)).ToList();
                    ftsc.SendMessageCarrier(new MessageCarrier(MessageType.ListFilesResponse) { Payload = fileList });
                }
                catch (Exception ex)
                {
                    ftsc.SendMessageCarrier(new MessageCarrier(MessageType.FileNotFound) { Payload = (mc.Payload as string) });
                }
            }

            if (mc.Type == MessageType.ListDirectoriesRequest)
            {
                var ftsc = sender as FileTunnelServerConnection;
                
                try
                {
                    // Run this just to throw exception if path doesn't exist
                    Path.GetFullPath(AddPaths(RootDirectory, mc.Payload as string));

                    var directoryList = Directory.GetDirectories(AddPaths(RootDirectory, mc.Payload as string)).ToList();

                    for (int i = 0; i < directoryList.Count; i++)
                        directoryList[i] = directoryList[i] + "\\";

                    ftsc.SendMessageCarrier(new MessageCarrier(MessageType.ListDirectoriesResponse) { Payload = directoryList });
                }
                catch (Exception ex)
                {
                    ftsc.SendMessageCarrier(new MessageCarrier(MessageType.FileNotFound) { Payload = (mc.Payload as string) });
                }
            }

            if (mc.Type == MessageType.FilePropertiesRequest)
            {
                var ftsc = sender as FileTunnelServerConnection;

                try
                {
                    // Run this just to throw exception if path doesn't exist
                    Path.GetFullPath(AddPaths(RootDirectory, mc.Payload as string));

                    var fileProperties = new FileProperties(AddPaths(RootDirectory, mc.Payload as string));
                    ftsc.SendMessageCarrier(new MessageCarrier(MessageType.FilePropertiesResponse) { Payload = fileProperties });
                }
                catch (Exception ex)
                {
                    ftsc.SendMessageCarrier(new MessageCarrier(MessageType.FileNotFound) { Payload = (mc.Payload as string) });
                }
            }
        }

        public void OnServerFileNotFound(object sender, FileRequestEventArgs e)
        {
            e.Connection.SendMessageCarrier(new MessageCarrier(MessageType.FileNotFound) { Payload = e.FilePathAndName });
        }

        public void OnServerSendFile(object sender, FileRequestEventArgs e)
        {
            FileStream fs = new FileStream(e.FilePathAndName, FileMode.Open, FileAccess.Read);
            byte[] bytes = File.ReadAllBytes(e.FilePathAndName);
            fs.Read(bytes, 0, Convert.ToInt32(fs.Length));
            fs.Close();

            e.Connection.SendMessageCarrier(new MessageCarrier(MessageType.FileContents) { Payload = bytes });
        }

        // To make sure that path doesn't get double or no backslash
        private string AddPaths(string p1, string p2)
        {
            if (p1.EndsWith("\\"))
            {
                if (p2.StartsWith("\\"))
                    return p1 + p2.Substring(1);

                else
                    return p1 + p2;
            }

            else
            {
                if (p2.StartsWith("\\"))
                    return p1 + p2;

                else
                {
                    return p1 + "\\" + p2;
                }
            }
        }
    }
}
