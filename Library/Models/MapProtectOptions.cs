using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Models
{
    public class MapProtectOptions
    {
        public int? SpamAmount { get; set; } = 5000;
        public bool IsRustEditDataProtectChecked { get; set; } = true;
        public bool IsDeployProtectChecked { get; set; } = true;
        public bool IsEditProtectChecked { get; set; } = true;
        public bool IsUploadMap { get; set; } = true;
    }
}
