using Library.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Extensions
{
    internal static class WorldSerializationExtension
    {
        public static void UpdatePassword(this WorldSerialization worldSerialization)
        {
            var passwordMap = worldSerialization.RetrievePasswordMap();

            if (passwordMap == null)
            {
                int prefabCount = worldSerialization.world.prefabs.Count;
                string encryptedPassword = WorldManager.Encrypt("mappassword", prefabCount);

                worldSerialization.world.maps.Add(new MapData()
                {
                    name = encryptedPassword,
                    data = new byte[0],
                });

                UpdatePassword(worldSerialization);
                return;
            }

            passwordMap.data = new byte[10];//(int)(int.MaxValue / 3)];
            for (int i = 0; i < passwordMap.data.Length; i++)
            {
                passwordMap.data[i] = 255;
            }
        }

        public static MapData RetrievePasswordMap(this WorldSerialization worldSerialization)
        {
            int prefabCount = worldSerialization.world.prefabs.Count;
            return worldSerialization.world.maps.FirstOrDefault(map =>
                map.name == DecryptPassword("mappassword", prefabCount) || map.name == WorldManager.Encrypt("mappassword", prefabCount)
            );
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
