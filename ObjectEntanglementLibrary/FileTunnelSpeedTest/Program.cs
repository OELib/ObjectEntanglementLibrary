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
            // string filePathAndName = @"C:\Users\ari\test.txt";
            string filePathAndName = @"C:\Users\ari\ReportBuilder3.msi";

            TestFileTransfer(1044, filePathAndName);
            Console.ReadLine();
        }
        
        public static FileDownloader fd1;
        public static FileDownloader fd2;
        public static FileDownloader fd3;

        public static void TestFileTransfer(int port, string filePathAndName)
        {
            FileServer fileServer = new FileServer("127.0.0.1", port);
            Thread.Sleep(1000);

            fd1 = new FileDownloader("127.0.0.1", port, @"C:\Users\ari\received\fd1\");
            fd2 = new FileDownloader("127.0.0.1", port, @"C:\Users\ari\received\fd2\");
            fd3 = new FileDownloader("127.0.0.1", port, @"C:\Users\ari\received\fd3\");

            Thread.Sleep(1000);

            // These should succeed if server responds (fire and forget)
                fd1.DownloadRequest(filePathAndName);
                fd2.DownloadRequest(filePathAndName);
                fd3.DownloadRequest(filePathAndName);

            // This should wait forever since thread is blocked until server responds
            // fd1.Download(filePathAndName);

            Thread.Sleep(1000);

            while (fileServer.RequestStack.Count > 0)
            {
                fileServer.RequestStack.PopAndProcess();
            }
        }
    }
}

