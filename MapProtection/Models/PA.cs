using MapUnlock.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapUnlock.Models
{
    public class PA
    {
        public uint ID;
        public string C;
        public string P;
        public string R;
        public string S;

        public PA New(uint id, string cat, WorldSerialization.VectorData p, WorldSerialization.VectorData r, WorldSerialization.VectorData s)
        {
            ID = id;
            C = cat;
            P = p.VectorData2String();
            R = r.VectorData2String();
            S = s.VectorData2String();
            return this;
        }
    }
}
