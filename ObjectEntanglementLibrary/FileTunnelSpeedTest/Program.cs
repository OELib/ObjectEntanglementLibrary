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
        static string TestReceiveDirectory = @"C:\Users\ari\TestFileServer\received\";
        static string TestServerRootDirectory = @"C:\Users\ari\TestFileServer\";

        static string TestServerSubDirectory = @"\";
        static string TestFile = "test.txt";

        static void Main(string[] args)
        {
            FileServer fileServer = new FileServer("127.0.0.1", 1044, TestServerRootDirectory);
            Thread.Sleep(1000);
            var fileDownloader = new FileDownloader("127.0.0.1", 1044, TestReceiveDirectory);
            Thread.Sleep(1000);

            Console.WriteLine("\n\nTestFileTransfer");
            TestFileTransfer(fileServer, fileDownloader, TestServerSubDirectory, TestFile);

            Console.WriteLine("\n\nTestListFiles");
            TestListFiles(fileDownloader, TestServerSubDirectory);

            Console.WriteLine("\n\nTestListDirectories");
            TestListDirectories(fileDownloader, TestServerSubDirectory);

            Console.WriteLine("\n\nTestListProperties");
            TestListFileProperties(fileDownloader, TestServerSubDirectory, TestFile);

            Console.ReadLine();
        }

        public static void TestFileTransfer(FileServer fileServer, FileDownloader fileDownloader, string serverDirectory, string fileName)
        {
            // This should succeed if server responds (fire and forget)
            fileDownloader.DownloadRequest(serverDirectory + fileName);

            // This should wait forever since thread is blocked until server responds
            // fd1.Download(filePathAndName);

            Thread.Sleep(1000);

            while (fileServer.RequestStack.Count > 0)
                fileServer.RequestStack.PopAndProcess();
        }

        public static void TestListFiles(FileDownloader fileDownloader, string serverDirectory)
        {
            fileDownloader.ListFiles(serverDirectory, out var fileList);

            foreach (string s in fileList)
                Console.WriteLine(s);
        }

        public static void TestListDirectories(FileDownloader fileDownloader, string serverDirectory)
        {
            fileDownloader.ListDirectories(serverDirectory, out var directoryList);

            foreach (string s in directoryList)
                Console.WriteLine(s);
        }

        public static void TestListFileProperties(FileDownloader fileDownloader, string serverDirectory, string fileName)
        {
            fileDownloader.GetFileProperties(serverDirectory + fileName, out var fileProperties);

            Console.WriteLine("Modified: " + fileProperties.Modified_String());
            Console.WriteLine("Size: " + fileProperties.Size_String());
            Console.WriteLine("MD5 Hash: " + fileProperties.MD5Hash_String());
        }
    }
}

