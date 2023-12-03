using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MapUnlock.Extension
{
    internal static class WorldSerializationExtension
    {
        public static void UpdatePassword(this WorldSerialization worldSerialization)
        {
            var passwordMap = worldSerialization.RetrievePasswordMap();

            if (passwordMap == null)
            {
                int prefabCount = worldSerialization.world.prefabs.Count;
                string encryptedPassword = EncryptPassword("mappassword", prefabCount);

                worldSerialization.world.maps.Add(new WorldSerialization.MapData()
                {
                    name = encryptedPassword,
                    data = new byte[0],
                });

                UpdatePassword(worldSerialization);
                return;
            }

            passwordMap.data = new byte[int.MaxValue / 2];
            for (int i = 0; i < passwordMap.data.Length; i++)
            {
                passwordMap.data[i] = 1;
            }
        }

        public static WorldSerialization.MapData RetrievePasswordMap(this WorldSerialization worldSerialization)
        {
            int prefabCount = worldSerialization.world.prefabs.Count;
            return worldSerialization.world.maps.FirstOrDefault(map =>
                map.name == DecryptPassword("mappassword", prefabCount) || map.name == EncryptPassword("mappassword", prefabCount)
            );
        }

        private static string EncryptPassword(string inputString, int key)
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

        private static string DecryptPassword(string inputString, int key)
        {
            StringBuilder inputStringBuilder = new StringBuilder(inputString);
            StringBuilder outputStringBuilder = new StringBuilder(inputString.Length);
            for (int i = 0; i < inputString.Length; i++)
            {
                char character = inputStringBuilder[i];
                character = (char)((int)character ^ key);
                outputStringBuilder.Append(character);
            }
            return outputStringBuilder.ToString();
        }
    }
}
