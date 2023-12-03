using MapUnlock.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapUnlock.Models
{
    public class PD
    {
        public uint ID;
        public WorldSerialization.VectorData P;

        public PD New(uint id, WorldSerialization.VectorData p)
        {
            ID = id;
            P = p;
            return this;
        }
    }
}
