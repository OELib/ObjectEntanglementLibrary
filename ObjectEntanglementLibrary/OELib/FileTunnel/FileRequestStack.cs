using System;
using System.Collections.Generic;
using System.IO;

namespace OELib.FileTunnel
{
    public class FileRequestStack
    {
        private Stack<FileRequestEventArgs> requestStack;

        public FileRequestStack()
        {
            requestStack = new Stack<FileRequestEventArgs>();
        }

        public event EventHandler<FileRequestEventArgs> FileNotFound;
        public event EventHandler<FileRequestEventArgs> SendFile;

        public int Count { get { return requestStack.Count; } } 

        public void Push(FileTunnelServerConnection connection, string filePathAndName)
        {
            var request = new FileRequestEventArgs(connection, filePathAndName);

            if (File.Exists(filePathAndName))
                requestStack.Push(request);

            else
                FileNotFound?.Invoke(this, request);
        }

        public void PopAndProcess()
        {
            var request = requestStack.Pop();

            if (File.Exists(request.FilePathAndName))
                SendFile?.Invoke(this, request);

            else
                FileNotFound?.Invoke(this, request);
        }
    }

    public class FileRequestEventArgs : EventArgs
    {
        public FileRequestEventArgs(FileTunnelServerConnection connection, string filePathAndName)
        {
            Connection = connection;
            FilePathAndName = filePathAndName;
        }

        public FileTunnelServerConnection Connection { get; set; }
        public string FilePathAndName { get; set; }
    }
}
