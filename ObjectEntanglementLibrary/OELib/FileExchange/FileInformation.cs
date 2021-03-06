﻿using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace OELib.FileExchange
{
    [Serializable]
    public class FileInformation
    {
        private readonly string _rootDirectory;
        public string FileName { get; }
        public string Directory { get; }
        public int Size { get; set; }
        public DateTime Created { get;  }
        public DateTime LastModified { get;  }
        public byte[] Hash { get; private set; }
        public bool Exists { get; private set; }

        /// <summary>
        /// File descriptor
        /// </summary>
        /// <param name="rootDir">Root directory (on server) - not transmitted to the clients</param>
        /// <param name="fileName">File name (including subdirectories beyond root dir) - transferred to clients</param>
        /// <param name="calculateHash">Calculate MD5 hash</param>
        public FileInformation(string rootDir, string fileName, bool calculateHash=false)
        {
            _rootDirectory = rootDir;
            var fullFileName = Path.Combine(rootDir, fileName);
            var fileInfo = new FileInfo(fullFileName);
            if (fileInfo.DirectoryName == null) return;
            Directory = Path.GetFullPath(fileInfo.DirectoryName + "\\").Substring(rootDir.Length);
            FileName = fileInfo.Name;
            Exists = fileInfo.Exists;
            if (!Exists) return;
            Created = fileInfo.CreationTime;
            LastModified = fileInfo.LastWriteTime;
            Size = (int) fileInfo.Length;
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
