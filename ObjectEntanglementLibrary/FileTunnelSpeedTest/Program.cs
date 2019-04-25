using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using OELib.ObjectTunnel;
using OELib.FileTunnel;
using System.IO;

namespace FileTunnelSpeedTest
{
    class Program
    {
        static string TestReceiveDirectory = @"C:\Users\ari\TestFileServer\received\fd1\";
        static string TestServerDirectory = @"C:\Users\ari\TestFileServer\";
        static string TestFile = "test.txt";

        static void Main(string[] args)
        {
            //Console.WriteLine("TestListFiles");
            //TestListFiles(1044, TestServerDirectory);

            //Console.WriteLine("\n\nTestListDirectories");
            //TestListDirectories(1044, TestServerDirectory);

            Console.WriteLine("\n\nTestFileTransfer");
            TestFileTransfer(1044, TestFile, TestServerDirectory);

            Console.ReadLine();
        }

        public static void TestFileTransfer(int port, string fileName, string serverDirectory)
        {
            FileServer fileServer = new FileServer("127.0.0.1", port);
            Thread.Sleep(1000);

            var fd1 = new FileDownloader("127.0.0.1", port, TestReceiveDirectory);

            Thread.Sleep(1000);

            // This should succeed if server responds (fire and forget)
            fd1.DownloadRequest(serverDirectory + fileName);

            // This should wait forever since thread is blocked until server responds
            // fd1.Download(filePathAndName);

            Thread.Sleep(1000);

            while (fileServer.RequestStack.Count > 0)
            {
                fileServer.RequestStack.PopAndProcess();
            }
        }

        public static void TestListFiles(int port, string serverDirectory)
        {
            FileServer fileServer = new FileServer("127.0.0.1", port);
            Thread.Sleep(1000);

            var fd1 = new FileDownloader("127.0.0.1", port, TestReceiveDirectory);
            Thread.Sleep(1000);

            var fileList = new List<string>();
            fd1.ListFiles(serverDirectory, out fileList);

            foreach (string s in fileList)
                Console.WriteLine(s);
        }

        public static void TestListDirectories(int port, string serverDirectory)
        {
            FileServer fileServer = new FileServer("127.0.0.1", port);
            Thread.Sleep(1000);

            var fd1 = new FileDownloader("127.0.0.1", port, TestReceiveDirectory);
            Thread.Sleep(1000);

            var directoryList = new List<string>();
            fd1.ListDirectories(serverDirectory, out directoryList);

            foreach (string s in directoryList)
                Console.WriteLine(s);
        }
    }
}

