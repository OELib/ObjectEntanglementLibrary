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
        // directories must end with \

        static string TestReceiveDirectory = @"C:\Users\ari\TestFileServer\received\";
        static string TestServerRootDirectory = @"C:\Users\ari\TestFileServer\";

        static string TestServerSubDirectory = @"testfolder1\";
        static string TestFile = "test.txt";

        static void Main(string[] args)
        {
            FileServer fileServer = new FileServer("127.0.0.1", 1044, TestServerRootDirectory);
            fileServer.FileTransferAutoProcess = true;
            Thread.Sleep(1000);

            var fileDownloader1 = new FileDownloader("127.0.0.1", 1044, TestReceiveDirectory);
            var fileDownloader2 = new FileDownloader("127.0.0.1", 1044, TestReceiveDirectory);
            var fileDownloader3 = new FileDownloader("127.0.0.1", 1044, TestReceiveDirectory);
            var fileDownloader4 = new FileDownloader("127.0.0.1", 1044, TestReceiveDirectory);
            var fileDownloader5 = new FileDownloader("127.0.0.1", 1044, TestReceiveDirectory);

            Console.WriteLine("\n\nTestFileTransfer - " + TestServerRootDirectory + TestServerSubDirectory + TestFile);
            TestFileTransfer(fileDownloader1, TestServerSubDirectory, TestFile);
            Console.WriteLine("Now check if " + TestFile + " is in " + TestReceiveDirectory);

            //Console.WriteLine("\n\nTestListFiles - " + TestServerRootDirectory + TestServerSubDirectory);
            //TestListFiles(fileDownloader1, TestServerSubDirectory);

            //Console.WriteLine("\n\nTestListDirectories - " + TestServerRootDirectory + TestServerSubDirectory);
            //TestListDirectories(fileDownloader1, TestServerSubDirectory);

            //Console.WriteLine("\n\nTestListProperties - " + TestServerRootDirectory + TestServerSubDirectory + TestFile);
            //Console.WriteLine("\nfileDownloader1");
            //TestListFileProperties(fileDownloader1, TestServerSubDirectory, TestFile);
            //Console.WriteLine("\nfileDownloader2");
            //TestListFileProperties(fileDownloader2, TestServerSubDirectory, TestFile);
            //Console.WriteLine("\nfileDownloader3");
            //TestListFileProperties(fileDownloader3, TestServerSubDirectory, TestFile);
            //Console.WriteLine("\nfileDownloader4");
            //TestListFileProperties(fileDownloader4, TestServerSubDirectory, TestFile);
            //Console.WriteLine("\nfileDownloader5");
            //TestListFileProperties(fileDownloader5, TestServerSubDirectory, TestFile);

            //// Run this, and in the TestServerRootDirectory try create a file, modify a file and rename a file.
            //// This should give 5 messages for each operation, total 15 messages in the console output.
            //Console.WriteLine("\n\nTestWatchDirectory - " + TestServerRootDirectory);
            //TestWatchDirectory(fileDownloader1);
            //TestWatchDirectory(fileDownloader2);
            //TestWatchDirectory(fileDownloader3);
            //TestWatchDirectory(fileDownloader4);
            //TestWatchDirectory(fileDownloader5);

            Console.ReadLine();
        }

        public static void TestFileTransfer(FileDownloader fileDownloader, string serverDirectory, string fileName)
        {
            fileDownloader.Download(serverDirectory + fileName);
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

            Console.WriteLine("File Name: " + fileName);
            Console.WriteLine("Modified: " + fileProperties.Modified_String());
            Console.WriteLine("Size: " + fileProperties.Size_String());
            Console.WriteLine("MD5 Hash: " + fileProperties.MD5Hash_String());
        }

        public static void TestWatchDirectory(FileDownloader fileDownloader)
        {
            fileDownloader.WatchDirectory(WatchDirectoryModified);
        }

        public static void WatchDirectoryModified(string filePathAndName)
        {
            Console.WriteLine("A file was modified: " + filePathAndName);
        }
    }
}

