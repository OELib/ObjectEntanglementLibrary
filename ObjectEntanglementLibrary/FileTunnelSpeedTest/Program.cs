using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using OELib.ObjectTunnel;
using OELib.FileTunnel;
using System.IO;
using System.Threading.Tasks;

namespace FileTunnelSpeedTest
{
    class Program
    {
        // directories must end with \

        static string TestReceiveDirectory = @"C:\Users\ari\TestFileServer\received\";
        static string TestServerRootDirectory = @"C:\Users\ari\TestFileServer\";

        static string TestServerSubDirectory = @"testfolder1\";

        static string TestFile1 = "test1.txt";
        static string TestFile2 = "test2.txt";
        static string TestFile3 = "test3.txt";
        static string TestFile4 = "test4.txt";
        static string TestFile5 = "test5.txt";

        static string TestBigFile1 = "testbig1.zip";
        static string TestBigFile2 = "testbig2.zip";
        static string TestBigFile3 = "testbig3.zip";
        static string TestBigFile4 = "testbig4.zip";
        static string TestBigFile5 = "testbig5.zip";

        static void Main(string[] args)
        {
            Console.WriteLine("This test needs 5 files " + TestFile1 + " - " + TestFile5 + " located in:");
            Console.WriteLine(TestServerRootDirectory + TestServerSubDirectory);
            Console.WriteLine("This test needs 5 big files " + TestBigFile1 + " - " + TestBigFile5 + " located in:");
            Console.WriteLine(TestServerRootDirectory + TestServerSubDirectory);
            Console.WriteLine("The download directory will be:");
            Console.WriteLine(TestReceiveDirectory);
            Console.WriteLine("To test ListFiles and ListFolders, also place some random files/folders in:");
            Console.WriteLine(TestServerRootDirectory + TestServerSubDirectory);
            Console.WriteLine("Press enter to continue...");
            Console.ReadLine();

            FileServer fileServer = new FileServer("127.0.0.1", 1044, TestServerRootDirectory);
            Thread.Sleep(1000);

            var fileDownloader1 = new FileDownloader("127.0.0.1", 1044, TestReceiveDirectory);
            var fileDownloader2 = new FileDownloader("127.0.0.1", 1044, TestReceiveDirectory);
            var fileDownloader3 = new FileDownloader("127.0.0.1", 1044, TestReceiveDirectory);
            var fileDownloader4 = new FileDownloader("127.0.0.1", 1044, TestReceiveDirectory);
            var fileDownloader5 = new FileDownloader("127.0.0.1", 1044, TestReceiveDirectory);

            // Uncomment the test you want to run

            //Console.WriteLine("\n\nTestFileTransfer - " + TestServerRootDirectory + TestServerSubDirectory + TestFile1);
            //TestFileTransfer(fileDownloader1, TestServerSubDirectory, TestFile1);
            //Console.WriteLine("Now check if " + TestFile1 + " is in " + TestReceiveDirectory);

            //Console.WriteLine("\n\nTestListFiles - " + TestServerRootDirectory + TestServerSubDirectory);
            //TestListFiles(fileDownloader1, TestServerSubDirectory);

            //Console.WriteLine("\n\nTestListDirectories - " + TestServerRootDirectory + TestServerSubDirectory);
            //TestListDirectories(fileDownloader1, TestServerSubDirectory);

            //Console.WriteLine("\n\nTestListProperties - " + TestServerRootDirectory + TestServerSubDirectory + TestFile1);
            //Console.WriteLine("\nfileDownloader1");
            //TestListFileProperties(fileDownloader1, TestServerSubDirectory, TestFile1);

            ////Run this, and in the TestServerRootDirectory try create a file, modify a file and rename a file.
            //// This should give 5 messages for each operation, total 15 messages in the console output.
            //Console.WriteLine("\n\nTestWatchDirectory - " + TestServerRootDirectory);
            //TestWatchDirectory(fileDownloader1);
            //TestWatchDirectory(fileDownloader2);
            //TestWatchDirectory(fileDownloader3);
            //TestWatchDirectory(fileDownloader4);
            //TestWatchDirectory(fileDownloader5);

            //// Test 5 fast downloads by the same client
            //Console.WriteLine("\n\nTestFileTransfer 5x - " + TestServerRootDirectory + TestServerSubDirectory + TestFile + " (1-5)");
            //TestFileTransfer(fileDownloader1, TestServerSubDirectory, "test1.txt");
            //TestFileTransfer(fileDownloader1, TestServerSubDirectory, "test2.txt");
            //TestFileTransfer(fileDownloader1, TestServerSubDirectory, "test3.txt");
            //TestFileTransfer(fileDownloader1, TestServerSubDirectory, "test4.txt");
            //TestFileTransfer(fileDownloader1, TestServerSubDirectory, "test5.txt");
            //Console.WriteLine("Now check if test1.txt - test5.txt are in " + TestReceiveDirectory);

            //// Test 5 fast download requests by different clients
            //Console.WriteLine("\n\nTestFileTransfer 5x - " + TestServerRootDirectory + TestServerSubDirectory + ", " + TestFile1 + " - " + TestFile5);
            //fileServer.FileTransferAutoProcess = false;
            //TestFileTransferRequest(fileDownloader1, TestServerSubDirectory, TestFile1);
            //TestFileTransferRequest(fileDownloader2, TestServerSubDirectory, TestFile2);
            //TestFileTransferRequest(fileDownloader3, TestServerSubDirectory, TestFile3);
            //TestFileTransferRequest(fileDownloader4, TestServerSubDirectory, TestFile4);
            //TestFileTransferRequest(fileDownloader5, TestServerSubDirectory, TestFile5);
            //Thread.Sleep(100);  // Give the server time to stack the requests
            //fileServer.ProcessAllFileTransferRequests();
            //fileServer.FileTransferAutoProcess = true;
            //Console.WriteLine("Now check if " + TestFile1 + " - " + TestFile5 + " are in " + TestReceiveDirectory);

            //Console.WriteLine("\n\nTestFileTransfer, non-existing file - " + TestServerRootDirectory + TestServerSubDirectory + "thisfiledoesnotexist.txt");
            //TestFileTransfer(fileDownloader1, TestServerSubDirectory, "thisfiledoesnotexist.txt");
            //Console.WriteLine("Now check if " + "thisfiledoesnotexist.txt" + " is in " + TestReceiveDirectory);

            //Console.WriteLine("\n\nTestBigFileTransfer, big file simultaneously to 5 clients - " + TestServerRootDirectory + TestServerSubDirectory + TestBigFile1);
            //var t1 = Task.Run(() => TestFileTransfer(fileDownloader1, TestServerSubDirectory, TestBigFile1));
            //var t2 = Task.Run(() => TestFileTransfer(fileDownloader2, TestServerSubDirectory, TestBigFile2));
            //var t3 = Task.Run(() => TestFileTransfer(fileDownloader3, TestServerSubDirectory, TestBigFile3));
            //var t4 = Task.Run(() => TestFileTransfer(fileDownloader4, TestServerSubDirectory, TestBigFile4));
            //var t5 = Task.Run(() => TestFileTransfer(fileDownloader5, TestServerSubDirectory, TestBigFile5));
            //t1.Wait();
            //t2.Wait();
            //t3.Wait();
            //t4.Wait();
            //t5.Wait();
            //Console.WriteLine("Now check if " + TestBigFile1 + " - " + TestBigFile5 + " are in " + TestReceiveDirectory);

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

