using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Models
{
    public class RE
    {
        public string H;
        public byte[] D;
        public int C;

        public RE New(string hash, byte[] data, int prefabcount)
        {
            H = hash;
            D = data;
            C = prefabcount;
            return this;
        }
    }
}
