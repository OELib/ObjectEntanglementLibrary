using OELib.UniversalConnection;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
//using OELib.UniversalConnection;

namespace UCExample
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



            var serverReactingObject = new ExampleReactingObject() { Name = "Server reacting object" };
            var clientReactingObject = new ExampleReactingObject() { Name = "Client reacting object" };
            var serverExampleObject = new ExampleObject() { Name = "Server example object" };
            var clientExampleObject = new ExampleObject() {Name = "Client example object"};
            var server = new UcServer(1024, serverReactingObject, serverFolder);
            server.ClientConnected += (_, __) => Console.WriteLine("A client connected to the server");
            server.Start();
            var client = new UCClientConnection(clientReactingObject, clientFolder);
            client.ObjectReceived += (sender, obj) => Console.WriteLine($"Client got an object: {obj}");
            server.ObjectReceived += (sender, obj) => Console.WriteLine($"Server got an object: {obj}");

            AutoResetEvent go = new AutoResetEvent(false);
            client.Started += (_, __) => go.Set();
            client.Start("127.0.0.1", 1024);
            go.WaitOne(10000);
            client.Reactor.CallRemoteMethod(OELib.LibraryBase.Priority.Normal, 100, "ExampleMethod",
                "Method executed on server, called on client");
            server.Connections.ForEach(c => c.Reactor.CallRemoteMethod(OELib.LibraryBase.Priority.Normal, 100,
                "ExampleMethod", "Method executed on client, called on server"));
            server.SendObject(serverExampleObject);
            client.SendObject(clientExampleObject);
            Thread.Sleep(100);

            Console.WriteLine("Testing files exchange");
            var fileListing = client.FileManager.ListRemoteFiles();
            Console.WriteLine("Remote file list retrieved:");
            // download all remote files
            fileListing.FileList.ForEach(f => Console.WriteLine($"File size:  {f.Size}, name {f.FileName}, dir {f.Directory}"));
            foreach (var f in fileListing.FileList)
            {
                var sw = Stopwatch.StartNew();
                var fileInfo = client.FileManager.DownloadFile(f);
                sw.Stop();
                if (fileInfo.Exists) Console.WriteLine($"Downloaded file {fileInfo.Name}, size {fileInfo.Length}. It took {sw.ElapsedMilliseconds} ms which is {fileInfo.Length / (sw.ElapsedMilliseconds / 1000.0)} byte / sec.");
            }

            client.FileManager.MonitorRemoteDirectoryChange();
            client.FileManager.RemoteFileCreated += (_, fi) => Console.WriteLine($"MonitorRemoteDirectoryChange: {fi.FileName} was created on server.");
            client.FileManager.RemoteFileDeleted += (_, fi) => Console.WriteLine($"MonitorRemoteDirectoryChange: {fi.FileName} was deleted on server.");
            client.FileManager.RemoteFileModified += (_, fi) => Console.WriteLine($"MonitorRemoteDirectoryChange: {fi.FileName} was modified on server.");

            Console.WriteLine("Remote directory monitor active. Writing a file in 1 second");

            Thread.Sleep(1000);
            var time = DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss");
            using (var writer = new StreamWriter(serverFolder + $"test_{time}_.txt", false)) writer.Write(time);

            Console.ReadLine();


            client.Stop();
            server.Stop();
            Console.WriteLine("Press any key to continue");
            Console.ReadKey();
        }
    }
}
