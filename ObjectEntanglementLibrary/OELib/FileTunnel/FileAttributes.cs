using System;
using System.IO;
using System.Security.Cryptography;

namespace OELib.FileTunnel
{
    public class FileAttributes
    {
        public FileAttributes(string filePathAndName)
        {
            DateTime Modified = File.GetLastWriteTime(filePathAndName);
            Size = new FileInfo(filePathAndName).Length;

            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filePathAndName))
                {
                    MD5Hash = md5.ComputeHash(stream);
                }
            }
        }

        public DateTime Modified { get; private set; }
        public long Size { get; private set; }
        public byte[] MD5Hash { get; private set; }
    }
}
