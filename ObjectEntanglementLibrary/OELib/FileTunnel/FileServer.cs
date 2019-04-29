using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace OELib.FileTunnel
{
    public class FileServer
    {
        public FileTunnelServer FileTunnelServer { get; set; }
        private FileRequestStack _fileTransferRequestStack;
        public string RootDirectory { get; set; }

        Timer _fileTransferAutoProcessTimer = new Timer(1000);
        private bool _fileTransferAutoProcess;
        public bool FileTransferAutoProcess
        {
            get
            {
                return _fileTransferAutoProcess;
            }
            set
            {
                _fileTransferAutoProcess = value;
                _fileTransferAutoProcessTimer.Enabled = _fileTransferAutoProcess;
            }
        }

        Timer _directoryWatcherTimer = new Timer(1000);
        private FileSystemWatcher _directoryWatcher;
        private List<FileTunnelServerConnection> _directoryWatcherSubscribers;

        // _directoryWatcherPending is needed because filesystemwatcher often fires twice when files are changed, known issue
        // Responses are only added here if they are not already in this list
        private Stack<MessageCarrier> _directoryWatcherPending = new Stack<MessageCarrier>();

        public FileServer(string ip, int port, string rootDirectory)
        {
            FileTunnelServer = new FileTunnelServer(new IPEndPoint(IPAddress.Parse("127.0.0.1"), port));
            FileTunnelServer.Start();

            _fileTransferRequestStack = new FileRequestStack();
            RootDirectory = rootDirectory;

            _fileTransferAutoProcessTimer.Elapsed += OnFileTransferTimerEvent;
            _fileTransferAutoProcessTimer.AutoReset = true;
            FileTransferAutoProcess = false;

            _directoryWatcherTimer.Elapsed += OnDirectoryWatcherTimerEvent;
            _directoryWatcherTimer.AutoReset = true;
            _directoryWatcherTimer.Enabled = true;

            _directoryWatcher = new FileSystemWatcher();
            _directoryWatcher.Path = RootDirectory;
            _directoryWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
            _directoryWatcher.Created += OnDirectoryWatcherChanged;
            _directoryWatcher.Changed += OnDirectoryWatcherChanged;
            _directoryWatcher.Renamed += OnDirectoryWatcherChanged;

            FileTunnelServer.MessageCarrierReceived += OnServerMessageCarrierReceived;
            _fileTransferRequestStack.SendFile += OnServerSendFile;
            _fileTransferRequestStack.FileNotFound += OnServerFileNotFound;
        }

        private void OnFileTransferTimerEvent(object source, ElapsedEventArgs e)
        {
            ProcessAllFileTransferRequests();
        }

        private void ProcessAllFileTransferRequests()
        {
            while (_fileTransferRequestStack.Count > 0)
                _fileTransferRequestStack.PopAndProcess();
        }

        private void OnDirectoryWatcherTimerEvent(object sender, ElapsedEventArgs e)
        {
            ProcessAllDirectoryWatcherRequests();
        }

        private void ProcessAllDirectoryWatcherRequests()
        {
            while (_directoryWatcherPending.Count > 0)
            {
                var mc = _directoryWatcherPending.Pop();

                foreach (var ftsc in _directoryWatcherSubscribers)
                {
                    ftsc.SendMessageCarrier(mc);
                }
            }
        }

        private void OnDirectoryWatcherChanged(object source, FileSystemEventArgs e)
        {
            var clientSidePathAndName = e.FullPath.Replace(RootDirectory, "");

            if (e.ChangeType == WatcherChangeTypes.Changed || e.ChangeType == WatcherChangeTypes.Created || e.ChangeType == WatcherChangeTypes.Renamed)
            {
                var mc = new MessageCarrier(MessageType.WatcherFileModified) { Payload = clientSidePathAndName };

                if (_directoryWatcherPending.Any(p => (string)p.Payload == clientSidePathAndName) == false)
                    _directoryWatcherPending.Push(mc);
            }
        }

        private void OnServerMessageCarrierReceived(object sender, MessageCarrier mc)
        {
            if (mc.Type == MessageType.FileRequest)
                _fileTransferRequestStack.Push(sender as FileTunnelServerConnection, AddPaths(RootDirectory, mc.Payload as string));

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

            if (mc.Type == MessageType.WatchDirectoryRequest)
            {
                var ftsc = sender as FileTunnelServerConnection;

                if (_directoryWatcherSubscribers == null)
                    _directoryWatcherSubscribers = new List<FileTunnelServerConnection>();

                _directoryWatcherSubscribers.Add(ftsc);

                if (_directoryWatcher.EnableRaisingEvents == false)
                    _directoryWatcher.EnableRaisingEvents = true;
            }
        }

        private void OnServerFileNotFound(object sender, FileRequestEventArgs e)
        {
            e.Connection.SendMessageCarrier(new MessageCarrier(MessageType.FileNotFound) { Payload = e.FilePathAndName });
        }

        private void OnServerSendFile(object sender, FileRequestEventArgs e)
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
