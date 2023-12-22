using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Library.Core
{
    internal class WorldManager
    {
        public static List<string> BlobIdentifiers = new List<string>()
        {
            "lootcontainerdata",
            "vendingdata",
            "ioentitydata",
            "oceanpathpoints",
           // "mappassword",
            "bradleypathpoints",
            "vehiclespawnpoints",
            "npcspawnpoints",
            "anchorpaths",
            "buildingblocks",
            "ziplines"
        };

        public static string? FindBlob(string encrypt, int key)
        {
            foreach (var blob in BlobIdentifiers)
            {
                if (Encrypt(blob, key) == encrypt)
                {
                    return blob;
                }
            }

            return null;
        }

        public static string Encrypt(string inputString, int key)
        {
            string password = key.ToString();
            byte[] bytes = Encoding.Unicode.GetBytes(inputString);
            using (Aes aes = Aes.Create())
            {
                Rfc2898DeriveBytes pwdGen = new Rfc2898DeriveBytes(password, new byte[]
                {
                    73, 118, 97, 110, 32, 77, 101, 100, 118, 101, 100, 101, 118
                });
                aes.Key = pwdGen.GetBytes(32);
                aes.IV = pwdGen.GetBytes(16);
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(bytes, 0, bytes.Length);
                        cryptoStream.Close();
                    }
                    inputString = Convert.ToBase64String(memoryStream.ToArray());
                }
            }
            return inputString;
        }
    }
}
