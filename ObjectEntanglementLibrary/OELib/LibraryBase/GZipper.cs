using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OELib.LibraryBase
{
    public class GZipper
    {
        public static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[40960];

            int cnt;
            Stopwatch sw = Stopwatch.StartNew();
            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
            sw.Stop();
            Console.WriteLine($"Stream copy took {sw.ElapsedMilliseconds} ms");
        }

        public static byte[] Zip(byte[] data)
        {
            //return data;
            Stopwatch sw = Stopwatch.StartNew();
            using (var msi = new MemoryStream(data))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionLevel.Optimal))
                {
                    CopyTo(msi, gs);
                }
                var compressedData = mso.ToArray();
                sw.Stop();
                Console.WriteLine($"Compressed {data.Length} bytes to {compressedData.Length} ({((double)compressedData.Length / data.Length * 100).ToString("F2")} %) in {sw.ElapsedMilliseconds} ms.");

                return compressedData;
            }
        }

        public static byte[] Unzip(byte[] bytes)
        {
            //return bytes;
            Stopwatch sw = Stopwatch.StartNew();
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    CopyTo(gs, mso);
                }

                sw.Stop();
                Console.WriteLine($"Decompressed {bytes.Length} bytes in {sw.ElapsedMilliseconds} ms.");

                return mso.ToArray();
            }
        }

    }
}
