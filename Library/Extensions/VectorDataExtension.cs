using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Extensions
{
    internal static class VectorDataExtension
    {
        public static string VectorData2String(this VectorData vectorData)
        {
            return vectorData.x.ToString(CultureInfo.InvariantCulture) + " " + vectorData.y.ToString(CultureInfo.InvariantCulture) + " " + vectorData.z.ToString(CultureInfo.InvariantCulture);
        }
    }
}
