using System;
using System.IO;
using System.Security.Cryptography;

namespace OELib.FileExchange
{
    public class FileInformation
    {
        private readonly string _rootDirectory;

        public Guid ID { get; }
        public string FileName { get; set; }
        public int Size { get; set; }
        public byte[] Hash { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastModified { get; set; }

        
        /// <summary>
        /// File descriptor
        /// </summary>
        /// <param name="rootDir">Root directory (on server) - not transmitted to the clients</param>
        /// <param name="fileName">File name (including subdirectories beyond root dir) - transferred to clients</param>
        /// <param name="calculateHash">Calculate MD5 hash</param>
        public FileInformation(string rootDir, string fileName, bool calculateHash=false)
        {
            ID = new Guid();
            _rootDirectory = rootDir;
            var fn = Path.Combine(rootDir, fileName);
            var fi = new FileInfo(fn);
            FileName = fileName;
            Created = fi.CreationTime;
            LastModified = fi.LastWriteTime;
            Size = (int) fi.Length;
            if (calculateHash)
                CalculateHash();
        }

        public void CalculateHash()
        {
            if (_rootDirectory == null)
                throw new NotSupportedException("Calculating hash is not allowed on this file.");
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(Path.Combine(_rootDirectory, FileName)))
                {
                    Hash = md5.ComputeHash(stream);
                }
            }
        }
    }
}
