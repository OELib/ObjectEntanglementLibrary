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
        static void Main(string[] args)
        {
            TestFileTransfer(1044, "test.txt", @"C:\Users\ari\TestFileServer\");
            TestListFiles(1044, @"C:\Users\ari\TestFileServer\");

            Console.ReadLine();
        }
        
        public static FileDownloader fd1;
        public static FileDownloader fd2;
        public static FileDownloader fd3;

        public static void TestFileTransfer(int port, string fileName, string serverDirectory)
        {
            FileServer fileServer = new FileServer("127.0.0.1", port);
            Thread.Sleep(1000);

            fd1 = new FileDownloader("127.0.0.1", port, @"C:\Users\ari\TestFileServer\received\fd1\");
            fd2 = new FileDownloader("127.0.0.1", port, @"C:\Users\ari\TestFileServer\received\fd2\");
            fd3 = new FileDownloader("127.0.0.1", port, @"C:\Users\ari\TestFileServer\received\fd3\");

            Thread.Sleep(1000);

            // These should succeed if server responds (fire and forget)
                fd1.DownloadRequest(serverDirectory + fileName);
                fd2.DownloadRequest(serverDirectory + fileName);
                fd3.DownloadRequest(serverDirectory + fileName);

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

            fd1 = new FileDownloader("127.0.0.1", port, @"C:\Users\ari\TestFileServer\received\fd1\");
            Thread.Sleep(1000);

            var fileList = new List<string>();
            fd1.ListFiles(serverDirectory, out fileList);

            foreach (string s in fileList)
                Console.WriteLine(s);
        }
    }
}

