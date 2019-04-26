using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace OELib.FileTunnel
{
    [Serializable]
    public class FileProperties
    {
        public FileProperties(string filePathAndName)
        {
            Modified = File.GetLastWriteTime(filePathAndName);
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

        public string Modified_String()
        {
            return Modified.ToString("G");
        }

        public string Size_String()
        {
            return Size.ToString();
        }

        public string MD5Hash_String()
        {
            StringBuilder result = new StringBuilder(MD5Hash.Length * 2);

            for (int i = 0; i < MD5Hash.Length; i++)
                result.Append(MD5Hash[i].ToString("X2"));

            return result.ToString();
        }
    }
}
