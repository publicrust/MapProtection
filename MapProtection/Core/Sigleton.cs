using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapProtection.Core
{
    internal abstract class Sigleton<T> where T : Sigleton<T>
    {
        public static T Instance;

        public Sigleton()
        {
            if (Instance == null)
            {
                Instance = (T)this;
                return;
            }

            throw new Exception("Dont create new Sigleton");
        }
    }
}
