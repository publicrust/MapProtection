using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Models
{
    public class PD
    {
        public uint ID;
        public VectorData P;

        public PD New(uint id, VectorData p)
        {
            ID = id;
            P = p;
            return this;
        }
    }
}
