using System;
using System.IO;
using System.Linq;
using System.Threading;
using OELib.FileExchange;

namespace FileConnExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var serverFolder = @".\ServerFiles\";
            var clientFolder = @".\ClientFiles\";
            //create 2 local dirs for server and a client.
            Directory.CreateDirectory(serverFolder);
            Directory.CreateDirectory(clientFolder);
            //create 2 files in the server dir.
            using (var writer = new StreamWriter(serverFolder + "test1.txt", false)) writer.Write("Test content file 1");
            using (var writer = new StreamWriter(serverFolder + "test2.txt", false)) writer.Write("Test content file 2");
            // start server
            var server = new FileExchangeServer(serverFolder, new System.Net.IPEndPoint(System.Net.IPAddress.Any, 2048));
            server.Start();
            //start the client as well
            var client = new FileExchangeClientConnection(clientFolder);
            AutoResetEvent go = new AutoResetEvent(false);
            client.Started += (_, __) => go.Set();
            client.Start("127.0.0.1", 2048);
            go.WaitOne(300);
            Console.WriteLine("Client connected");
            //list all remote files
            var fileListing = client.FileManager.ListRemoteFiles();
            Console.WriteLine("Remote file list retrieved:");
            // download all remote files
            fileListing.FileList.ForEach(f => Console.WriteLine($"File size:  {f.Size}, name {f.FileName}, dir {f.Directory}"));
            foreach (var f in fileListing.FileList)
            {
                var fileInfo = client.FileManager.DownloadFile(f);
                if (fileInfo.Exists) Console.WriteLine($"Downloaded file {fileInfo.Name}, size {fileInfo.Length}.");
            }

            client.FileManager.MonitorRemoteDirectory();
            client.FileManager.RemoteFileCreated += (_, fi) => Console.WriteLine($"{fi.FileName} was created on server.");
            client.FileManager.RemoteFileDeleted += (_, fi) => Console.WriteLine($"{fi.FileName} was deleted on server.");
            client.FileManager.RemoteFileModified += (_, fi) => Console.WriteLine($"{fi.FileName} was modified on server.");

            Console.ReadLine();


            client.Stop();
        }
    }
}
