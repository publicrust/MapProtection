using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MapUnlock.Core
{
    internal class PrefabEntity
    {
        public List<uint> Esnts = new List<uint>();

        public PrefabEntity()
        {
            GetPrefabIDS();
        }

        public bool IsEntity(uint EntityID)
        {
            if (Esnts.Contains(EntityID)) { return true; }
            return false;
        }

        private void GetPrefabIDS()
        {
            string[] prefabs = Encoding.UTF8.GetString(Compression.Uncompress(Convert.FromBase64String("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"))).Split(',');

            foreach (string prefab in prefabs)
            {
                uint p = 0;
                if (uint.TryParse(prefab, out p)) { Esnts.Add(p); }
            }
        }
    }
}
