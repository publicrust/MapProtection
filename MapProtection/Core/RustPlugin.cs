using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MapUnlock.Core
{
    internal class RustPlugin
    {
        private string? _plugin = @"
// Reference: 0Harmony
using Facepunch.Utility;
using Harmony;
using Newtonsoft.Json;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info(""MapProtection"", ""bmgjet & FREE RUST"", ""1.0.1"")]
    [Description(""MapProtection"")]
    class MapProtection : RustPlugin
    {
        public static MapProtection plugin;
        private HarmonyInstance _harmony; //Reference to harmony
        string Key = @""%PREFABKEY%"";
        string ADD = @""%ADDKEY%"";
        string RED = @""%REKEY%"";
        uint MapSize;
        List<PD> RemovePrefabs = new List<PD>();
        List<PA> AddPrefabs = new List<PA>();
        List<RE> AddRE = new List<RE>();
        public class RE { public string H; public byte[] D; public int C; }
        public class PD { public uint ID; public Vector3 P; }
        public class PA { public uint ID; public string C; public string P; public string R; public string S; }
        private void Init()
        {
            plugin = this;
            MapSize = uint.Parse(""%SIZE%"");
            if(Key.Length > 10)
                RemovePrefabs = JsonConvert.DeserializeObject<List<PD>>(Encoding.UTF8.GetString(Compression.Uncompress(Convert.FromBase64String(Key))));
            if(ADD.Length > 10)           
                AddPrefabs = JsonConvert.DeserializeObject<List<PA>>(Encoding.UTF8.GetString(Compression.Uncompress(Convert.FromBase64String(ADD))));
            if(RED.Length > 10)
                AddRE = JsonConvert.DeserializeObject<List<RE>>(Encoding.UTF8.GetString(Compression.Uncompress(Convert.FromBase64String(RED))));
            _harmony = HarmonyInstance.Create(Name + ""PATCH"");
            Type[] patchType = { AccessTools.Inner(typeof(MapProtection), ""OnWorldLoad_hook""), };
            foreach (var t in patchType) { new PatchProcessor(_harmony, t, HarmonyMethod.Merge(t.GetHarmonyMethods())).Patch(); }
            
            Key = """";
            ADD = """";
            RED = """";
        }
        private void OnTerrainCreate() { World.Size = MapSize; ConVar.Server.worldsize = (int)MapSize; }
        private void OnServerInitialized() { timer.Once(10, () => { covalence.Server.Command(""o.unload"", this.Name); }); }
        private void Unload() { _harmony.UnpatchAll(Name + ""PATCH""); plugin = null; }
        public string VectorData2String(VectorData vectorData) { return vectorData.x.ToString(CultureInfo.InvariantCulture) + "" "" + vectorData.y.ToString(CultureInfo.InvariantCulture) + "" "" + vectorData.z.ToString(CultureInfo.InvariantCulture); }
        public VectorData String2Vector(string vectorData)
        {
            string[] s = vectorData.Split(new[] { "" "" }, StringSplitOptions.None);
            return new VectorData(float.Parse(s[0], CultureInfo.InvariantCulture), float.Parse(s[1], CultureInfo.InvariantCulture), float.Parse(s[2], CultureInfo.InvariantCulture));
        }
        [HarmonyPatch(typeof(WorldSerialization), nameof(WorldSerialization.Load), typeof(string))]
        internal class OnWorldLoad_hook
        {
            [HarmonyPostfix]
            static void Postfix(WorldSerialization __instance)
            {
                var mapData = GetPasswordMap(__instance);

                mapData.data = new byte[0];
                __instance.GetMap(""hieght"").data = new byte[0];

                for (int i = 0; i < __instance.world.prefabs.Count; i++)
                {
                    if (1724395471 == __instance.world.prefabs[i].id)
                        continue;

                    __instance.world.prefabs[i].category = ""Decor"";
                }

                int D = 0;
                int A = 0;
                for (int i = __instance.world.prefabs.Count - 1; i >= 0; i--)
                {
                    for (int p = plugin.RemovePrefabs.Count - 1; p >= 0; p--)
                    {
                        try
                        {
                            if (plugin.RemovePrefabs[p].ID == __instance.world.prefabs[i].id && plugin.RemovePrefabs[p].P == __instance.world.prefabs[i].position)
                            {
                                __instance.world.prefabs.RemoveAt(i);
                                plugin.RemovePrefabs.RemoveAt(p);
                                D++;
                                break;
                            }
                        }
                        catch { }
                    }
                }
                if (plugin.AddPrefabs.Count > 0)
                {
                    foreach (var p in plugin.AddPrefabs)
                    {
                        PrefabData pd = new PrefabData();
                        pd.id = p.ID;
                        pd.category = p.C;
                        pd.position = plugin.String2Vector(p.P);
                        pd.rotation = plugin.String2Vector(p.R);
                        pd.scale = plugin.String2Vector(p.S);
                        __instance.world.prefabs.Add(pd);
                        A++;
                    }
                }
                if (plugin.AddRE.Count > 0)
                {
                    plugin.Puts(""Loading RustEditData"");
                    int PC = __instance.world.prefabs.Count;
                    foreach (var p in plugin.AddRE)
                    {
                        if (p.C != PC) { plugin.Puts(""Invalid Prefab Count "" + p.C + "" / "" + PC); break; }
                        __instance.AddMap(p.H, p.D);
                    }
                }
                UnityEngine.Debug.LogWarning(""[Map Protecion] Removed "" + D + "" Spam Prefabs / Restored "" + A + "" Missing Prefabs"");
            }

            private static MapData GetPasswordMap(WorldSerialization worldSerialization)
            {
                int prefabsCount = worldSerialization.world.prefabs.Count;
                return worldSerialization.world.maps.FirstOrDefault(s => s.name == OPIPGDGHLCP(""mappassword"", prefabsCount) || s.name == EIGDJDKLLED(""mappassword"", prefabsCount));
            }

            private static string EIGDJDKLLED(string KBJLCHGLCPN, int FNCNKFNPJAB)
            {
                string password = FNCNKFNPJAB.ToString();
                byte[] bytes = Encoding.Unicode.GetBytes(KBJLCHGLCPN);
                using (Aes aes = Aes.Create())
                {
                    Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, new byte[]
                    {
                73,
                118,
                97,
                110,
                32,
                77,
                101,
                100,
                118,
                101,
                100,
                101,
                118
                    });
                    aes.Key = rfc2898DeriveBytes.GetBytes(32);
                    aes.IV = rfc2898DeriveBytes.GetBytes(16);
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            cryptoStream.Write(bytes, 0, bytes.Length);
                            cryptoStream.Close();
                        }
                        KBJLCHGLCPN = Convert.ToBase64String(memoryStream.ToArray());
                    }
                }
                return KBJLCHGLCPN;
            }

            private static string OPIPGDGHLCP(string JKNFEDNIELG, int FNCNKFNPJAB)
            {
                StringBuilder stringBuilder = new StringBuilder(JKNFEDNIELG);
                StringBuilder stringBuilder2 = new StringBuilder(JKNFEDNIELG.Length);
                for (int i = 0; i < JKNFEDNIELG.Length; i++)
                {
                    char c = stringBuilder[i];
                    c = (char)((int)c ^ FNCNKFNPJAB);
                    stringBuilder2.Append(c);
                }
                return stringBuilder2.ToString();
            }
        }
    }
}
";

        public string Plugin
        {
            get
            {
                //if (string.IsNullOrEmpty(_plugin))
                //{
                //    _plugin = Encoding.UTF8.GetString(Compression.Uncompress(Convert.FromBase64String(GetPluginTemplate())));
                //}

                return _plugin;
            }
        }

        //private string GetPluginTemplate()
        //{
        //    WebClient wc = new WebClient();
        //    wc.Headers["User-Agent"] = Web.Useragent;
        //    wc.Headers[HttpRequestHeader.ContentType] = Web.Header;
        //    string result = wc.UploadString(Web.Uri, "plugin=download");
        //    wc.Dispose();
        //    return result;
        //}
    }

}
