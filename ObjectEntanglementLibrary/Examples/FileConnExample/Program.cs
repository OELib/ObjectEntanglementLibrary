using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using OELib.FileConnection;

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
            using (var writer = new StreamWriter("test1.txt", false)) writer.Write("Test content file 1");
            using (var writer = new StreamWriter("test2.txt", false)) writer.Write("Test content file 2");
            // start server
            var server = new FileConnectionServer(serverFolder, new System.Net.IPEndPoint(System.Net.IPAddress.Any, 2048));
            server.Start();
            var client = new FileConnectionClient


        }
    }
}
