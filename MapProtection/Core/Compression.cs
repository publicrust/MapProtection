using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapUnlock.Core
{
    public class Compression
    {
        public static byte[] Compress(byte[] data)
        {
            try
            {
                return Ionic.Zlib.GZipStream.CompressBuffer(data);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static byte[] Uncompress(byte[] data)
        {
            return Ionic.Zlib.GZipStream.UncompressBuffer(data);
        }
    }
}
