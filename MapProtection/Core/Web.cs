using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapUnlock.Core
{
    internal class Web
    {
        public static readonly string Uri = @"https://raw.githubusercontent.com/bmgjet/MapProtection/master/Data.config";
        public const string Header = "application/x-www-form-urlencoded";
        public const string Useragent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/117.0";

        static Web()
        {
            using (var webClient = new System.Net.WebClient()) 
            {
                Uri = Encoding.UTF8.GetString(Compression.Uncompress(Convert.FromBase64String(webClient.DownloadString(Uri)))); 
            }
        }
    }
}
