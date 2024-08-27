using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Core
{
    public static class RustPlugin
    {
        public const string Plugin = @"
// Reference: 0Harmony
using CompanionServer.Handlers;
using Facepunch.Utility;
using HarmonyLib;
using Newtonsoft.Json;
using ProtoBuf;
using SilentOrbit.ProtocolBuffers;
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
    [Info(""MapProtection"", ""bmgjet & FREE RUST"", ""1.1.7"")]
    [Description(""MapProtection"")]
    class MapProtection : RustPlugin
    {
        public static MapProtection plugin;
        private Harmony _harmony;
        const string Key = @""%ROOT%"";

        private Lazy<RootModel> _root;

        private void Loaded()
        {
            plugin = this;

            if (Key.Length > 10)
                _root = new Lazy<RootModel>(() => JsonConvert.DeserializeObject<RootModel>(Encoding.UTF8.GetString(Compression.Uncompress(Convert.FromBase64String(Key)))));

            if (!string.IsNullOrWhiteSpace(_root.Value.DownloadUrl))
            {
                ConVar.Server.levelurl = _root.Value.DownloadUrl;
                World.Url = _root.Value.DownloadUrl;
            }

            _harmony = new Harmony(Name + ""PATCH"");

            Type[] patchType = {
                AccessTools.Inner(typeof(MapProtection), ""OnWorldLoad_hook""),
                AccessTools.Inner(typeof(MapProtection), ""DeserializeLengthDelimited_hook"")
            };

            _harmony.PatchAll();
        }

        public List<HarmonyMethod> GetHarmonyMethods(Type type)
        {
            return (from HarmonyAttribute attr in
                        from attr in type.GetCustomAttributes(true)
                        where attr is HarmonyAttribute
                        select attr
                    select attr.info).ToList<HarmonyMethod>();
        }

        private void OnTerrainCreate() { World.Size =  _root.Value.Size; ConVar.Server.worldsize = (int) _root.Value.Size; }
        private void OnServerInitialized() { timer.Once(10, () => { covalence.Server.Command(""o.unload"", this.Name); }); }
        private void Unload() { _harmony.UnpatchAll(Name + ""PATCH""); plugin = null; }
        public string VectorData2String(VectorData vectorData) { return vectorData.x.ToString(CultureInfo.InvariantCulture) + "" "" + vectorData.y.ToString(CultureInfo.InvariantCulture) + "" "" + vectorData.z.ToString(CultureInfo.InvariantCulture); }
        public VectorData String2Vector(string vectorData)
        {
            string[] s = vectorData.Split(new[] { "" "" }, StringSplitOptions.None);
            return new VectorData(float.Parse(s[0], CultureInfo.InvariantCulture), float.Parse(s[1], CultureInfo.InvariantCulture), float.Parse(s[2], CultureInfo.InvariantCulture));
        }
        public Vector3 StringToVector3(string vectorData)
        {
            string[] s = vectorData.Split(new[] { "" "" }, StringSplitOptions.None);
            if (s.Length >= 3)
            {
                float x = float.Parse(s[0], CultureInfo.InvariantCulture);
                float y = float.Parse(s[1], CultureInfo.InvariantCulture);
                float z = float.Parse(s[2], CultureInfo.InvariantCulture);
                return new Vector3(x, y, z);
            }
            else
            {
                Debug.LogWarning(""StringToVector3: Неверный формат входных данных.Возвращен Vector3.zero."");
                return Vector3.zero;
            }
        }

        [HarmonyPatch(typeof(MapData), nameof(MapData.DeserializeLengthDelimited), typeof(Stream), typeof(MapData), typeof(bool))]
        internal class DeserializeLengthDelimited_hook
        {
            static bool Prefix(Stream stream, MapData instance, bool isDelta, ref MapData __result)
            {
                long num = ProtocolParser.ReadUInt32(stream);
                num += stream.Position;
                while (stream.Position < num)
                {
                    int num2 = stream.ReadByte();

                    if (num2 == -1)
                    {
                        throw new EndOfStreamException();
                    }
                    else if (num2 == 10)
                    {
                        instance.name = ProtocolParser.ReadString(stream);
                    }
                    else if (num2 == 18)
                    {
                        if (instance.name == ""hieght"" || instance.name == "" % MAPPASSWORD % "")
                        {
                            instance.data = new byte[0];
                            ProtocolParser.SkipBytes(stream);
                        }
                        else
                        {
                            instance.data = ProtocolParser.ReadBytes(stream);
                        }

                    }
                    else
                    {
                        Key key = ProtocolParser.ReadKey((byte)num2, stream);
                        if (key.Field == 0)
                        {
                            throw new ProtocolBufferException(""Invalid field id: 0, something went wrong in the stream"");
                        }

                        ProtocolParser.SkipKey(stream, key);
                    }

                    Debug.Log($""[Map Protecion] DeserializeLengthDelimited_hook памяти было затрачено { System.GC.GetAllocatedBytesForCurrentThread()}"");
                }

                if (stream.Position != num)
                {
                    throw new ProtocolBufferException(""Read past max limit"");
                }

                __result = instance;
                return false;
            }
        }

        [HarmonyPatch(typeof(WorldSerialization), nameof(WorldSerialization.Load), typeof(string))]
        internal class OnWorldLoad_hook
        {
            [HarmonyPostfix]
            static void Postfix(WorldSerialization __instance)
            {
                var mapData = GetPasswordMap(__instance);

                if (mapData != null)
                    mapData.data = new byte[0];

                var hieght = __instance.GetMap(""hieght"");

                if (hieght != null)
                    hieght.data = new byte[0];

                for (int i = 0; i < __instance.world.prefabs.Count; i++)
                {
                    if (1724395471 == __instance.world.prefabs[i].id)
                        continue;

                    __instance.world.prefabs[i].category = ""Decor"";
                }

                int D = 0;
                int A = 0;


                if (plugin._root.Value.RemovePrefabs != null && plugin._root.Value.RemovePrefabs.Count > 0)
                {
                    for (int i = __instance.world.prefabs.Count - 1; i >= 0; i--)
                    {
                        for (int p = plugin._root.Value.RemovePrefabs.Count - 1; p >= 0; p--)
                        {
                            try
                            {
                                if (plugin._root.Value.RemovePrefabs[p].ID == __instance.world.prefabs[i].id && plugin._root.Value.RemovePrefabs[p].P == __instance.world.prefabs[i].position)
                                {
                                    __instance.world.prefabs.RemoveAt(i);
                                    plugin._root.Value.RemovePrefabs.RemoveAt(p);
                                    D++;
                                    break;
                                }
                            }
                            catch { }
                        }
                    }
                }

                if (plugin._root.Value.AddPathData != null && plugin._root.Value.AddPathData.Count > 0)
                {
                    foreach (var p in plugin._root.Value.AddPathData)
                    {
                        var pathData = __instance.world.paths.FirstOrDefault(s => s.name == p.Name);
                        if (pathData == null)
                        {
                            plugin.Puts(""path not found "" + p.Name);
                            continue;
                        }

                        pathData.hierarchy = p.Hierarchy;
                    }
                }

                if (plugin._root.Value.AddPrefabs != null && plugin._root.Value.AddPrefabs.Count > 0)
                {
                    foreach (var p in plugin._root.Value.AddPrefabs)
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

                if (plugin._root.Value.AllPrefabs != null && plugin._root.Value.AllPrefabs.Count > 0)
                {
                    foreach (var p in plugin._root.Value.AllPrefabs)
                    {
                        var prefab = __instance.world.prefabs.FirstOrDefault(s => s.id == p.ID && s.position == plugin.StringToVector3(p.P) && s.rotation == plugin.StringToVector3(p.R) && s.scale == plugin.StringToVector3(p.S));

                        if (prefab == null)
                        {
                            plugin.Puts($""Prefab {p.ID} not found"");
                        }
                        else
                        {
                            prefab.category = p.C;
                        }
                    }
                }

                if (plugin._root.Value.AddRE.Count > 0)
                {
                    plugin.Puts(""Loading RustEditData"");
                    int PC = __instance.world.prefabs.Count;
                    foreach (var p in plugin._root.Value.AddRE)
                    {
                        if (p.C != PC) { plugin.Puts(""Invalid Prefab Count "" + p.C + "" / "" + PC); break; }
                        __instance.AddMap(p.H, p.D);
                    }
                }

                plugin._root = null;
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

        class RootModel
        {
            public List<PD> RemovePrefabs { get; set; }
            public List<PA> AddPrefabs { get; set; }
            public List<RE> AddRE { get; set; }
            public List<PA> AllPrefabs { get; set; }
            public List<PathDataModel> AddPathData { get; set; }
            public uint Size { get; set; }
            public string DownloadUrl { get; set; }
        }

        public class RE { public string H; public byte[] D; public int C; }
        public class PD { public uint ID; public Vector3 P; }
        public class PA { public uint ID; public string C; public string P; public string R; public string S; }

        internal class PathDataModel
        {
            public string Name { get; set; }
            public int Hierarchy { get; set; }
        }
    }
}
";
    }
}
