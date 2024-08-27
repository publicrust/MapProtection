using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Library.Models
{
    internal class RootModel
    {
        public List<PD> RemovePrefabs { get; set; }
        public List<PA> AddPrefabs { get; set; }
        public List<RE> AddRE { get; set; }
        public List<PA> AllPrefabs { get; set; }
        public List<PathDataModel> AddPathData { get; set; }
        public uint Size { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? DownloadUrl { get; set; }
    }
}
