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
    [Info(""MapProtection[%MapName%]"", ""bmgjet & FREE RUST"", ""1.1.8"")]
    [Description(""MapProtection"")]
    internal sealed class MapProtection : RustPlugin
    {
        private static MapProtection? plugin;
        private Harmony? _harmony;
        private const string Key = ""%ROOT%"";

        private Lazy<RootModel>? _root;
        internal static readonly string[] separator = new[] { "" "" };

        private static void SetPlugin(MapProtection? instance)
        {
            plugin = instance;
        }

        private static void SetLevelUrl(string url)
        {
            ConVar.Server.levelurl = url;
            World.Url = url;
        }

        private static void SetWorldSize(uint size)
        {
            World.Size = size;
            ConVar.Server.worldsize = (int)size;
        }

        private void Loaded()
        {
            SetPlugin(this);

            if (Key.Length > 10)
            {
                _root = new Lazy<RootModel>(() => JsonConvert.DeserializeObject<RootModel>(Encoding.UTF8.GetString(Compression.Uncompress(Convert.FromBase64String(Key)))));
            }

            if (_root?.Value?.DownloadUrl is { } downloadUrl && !string.IsNullOrWhiteSpace(downloadUrl))
            {
                SetLevelUrl(downloadUrl);
            }

            _harmony = new Harmony(Name + ""PATCH"");
            _harmony.PatchAll();
        }

        private void OnTerrainCreate()
        {
            if (_root?.Value != null)
            {
                SetWorldSize(_root.Value.Size);
                _root = null;
            }
        }

        private void OnServerInitialized(bool initial)
        {
            _ = timer.Once(10, () => covalence.Server.Command(""o.unload"", Name));
        }

        private void Unload()
        {
            if (_harmony != null)
            {
                _harmony.UnpatchAll(Name + ""PATCH"");
                _harmony = null;
            }
            SetPlugin(null);
        }

        private VectorData String2Vector(string vectorData)
        {
            string[] s = vectorData.Split(separator, StringSplitOptions.None);
            return new VectorData(float.Parse(s[0], CultureInfo.InvariantCulture), float.Parse(s[1], CultureInfo.InvariantCulture), float.Parse(s[2], CultureInfo.InvariantCulture));
        }

        private Vector3 StringToVector3(string vectorData)
        {
            string[] s = vectorData.Split(separator, StringSplitOptions.None);
            if (s.Length >= 3)
            {
                float x = float.Parse(s[0], CultureInfo.InvariantCulture);
                float y = float.Parse(s[1], CultureInfo.InvariantCulture);
                float z = float.Parse(s[2], CultureInfo.InvariantCulture);
                return new Vector3(x, y, z);
            }
            Debug.LogWarning(""StringToVector3: Invalid input format. Returning Vector3.zero."");
            return Vector3.zero;
        }

        [HarmonyPatch(typeof(MapData), ""DeserializeLengthDelimited"")]
        internal static class DeserializeLengthDelimitedHook
        {
            private static bool Prefix(Stream stream, MapData instance, bool isDelta, ref MapData __result)
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
                        if (instance.name is ""hieght"" or ""%MAPPASSWORD%"")
                        {
                            instance.data = Array.Empty<byte>();
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

                    Debug.Log($""[Map Protection] DeserializeLengthDelimited_hook memory used: {GC.GetAllocatedBytesForCurrentThread()}"");
                }

                if (stream.Position != num)
                {
                    throw new ProtocolBufferException(""Read past max limit"");
                }

                __result = instance;
                return false;
            }
        }

        [HarmonyPatch(typeof(WorldSerialization), ""Load"")]
        internal static class OnWorldLoadHook
        {
            [HarmonyPostfix]
            private static void Postfix(WorldSerialization __instance)
            {
                MapData? mapData = GetPasswordMap(__instance);

                if (mapData != null)
                {
                    mapData.data = Array.Empty<byte>();
                }

                MapData? height = __instance.GetMap(""hieght"");

                if (height != null)
                {
                    height.data = Array.Empty<byte>();
                }

                for (int i = 0; i < __instance.world.prefabs.Count; i++)
                {
                    if (__instance.world.prefabs[i].id == 1724395471)
                    {
                        continue;
                    }

                    __instance.world.prefabs[i].category = ""Decor"";
                }

                int removedCount = 0;
                int addedCount = 0;

                if (plugin?._root?.Value.RemovePrefabs?.Count > 0)
                {
                    for (int i = __instance.world.prefabs.Count - 1; i >= 0; i--)
                    {
                        for (int p = plugin._root.Value.RemovePrefabs.Count - 1; p >= 0; p--)
                        {
                            try
                            {
                                if (plugin._root.Value.RemovePrefabs[p].ID == __instance.world.prefabs[i].id &&
                                    plugin._root.Value.RemovePrefabs[p].P == __instance.world.prefabs[i].position)
                                {
                                    __instance.world.prefabs.RemoveAt(i);
                                    plugin._root.Value.RemovePrefabs.RemoveAt(p);
                                    removedCount++;
                                    break;
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.LogError($""Error removing prefab: {ex.Message}"");
                            }
                        }
                    }
                }

                if (plugin?._root?.Value.AddPathData?.Count > 0)
                {
                    foreach (PathDataModel p in plugin._root.Value.AddPathData)
                    {
                        PathData? pathData = __instance.world.paths.FirstOrDefault(s => s.name == p.Name);
                        if (pathData == null)
                        {
                            Debug.Log($""Path not found: {p.Name}"");
                            continue;
                        }

                        pathData.hierarchy = p.Hierarchy;
                    }
                }

                if (plugin?._root?.Value.AddPrefabs?.Count > 0)
                {
                    foreach (PA p in plugin._root.Value.AddPrefabs)
                    {
                        PrefabData pd = new()
                        {
                            id = p.ID,
                            category = p.C,
                            position = plugin.String2Vector(p.P),
                            rotation = plugin.String2Vector(p.R),
                            scale = plugin.String2Vector(p.S)
                        };
                        __instance.world.prefabs.Add(pd);
                        addedCount++;
                    }
                }

                if (plugin?._root?.Value.AllPrefabs?.Count > 0)
                {
                    foreach (PA p in plugin._root.Value.AllPrefabs)
                    {
                        PrefabData? prefab = __instance.world.prefabs.FirstOrDefault(s =>
                            s.id == p.ID &&
                            s.position == plugin.StringToVector3(p.P) &&
                            s.rotation == plugin.StringToVector3(p.R) &&
                            s.scale == plugin.StringToVector3(p.S));

                        if (prefab == null)
                        {
                            Debug.Log($""Prefab {p.ID} not found"");
                        }
                        else
                        {
                            prefab.category = p.C;
                        }
                    }
                }

                if (plugin?._root?.Value.AddRE?.Count > 0)
                {
                    Debug.Log(""Loading RustEditData"");
                    int prefabCount = __instance.world.prefabs.Count;
                    foreach (RE p in plugin._root.Value.AddRE)
                    {
                        if (p.C != prefabCount)
                        {
                            Debug.Log($""Invalid Prefab Count {p.C} / {prefabCount}"");
                            break;
                        }
                        __instance.AddMap(p.H, p.D);
                    }
                }

                Debug.LogWarning($""[Map Protection] Removed {removedCount} Spam Prefabs / Restored {addedCount} Missing Prefabs"");
            }

            private static MapData? GetPasswordMap(WorldSerialization worldSerialization)
            {
                int prefabsCount = worldSerialization.world.prefabs.Count;
                return worldSerialization.world.maps.FirstOrDefault(s =>
                    s.name == OPIPGDGHLCP(""mappassword"", prefabsCount) ||
                    s.name == EIGDJDKLLED(""mappassword"", prefabsCount));
            }

            private static string EIGDJDKLLED(string input, int count)
            {
                string password = count.ToString(CultureInfo.InvariantCulture);
                byte[] bytes = Encoding.Unicode.GetBytes(input);
                using Aes aes = Aes.Create();
                using Rfc2898DeriveBytes deriveBytes = new(password, ""Ivan Medvedev""u8.ToArray(), 100000, HashAlgorithmName.SHA512);
                aes.Key = deriveBytes.GetBytes(32);
                aes.IV = deriveBytes.GetBytes(16);
                using MemoryStream memoryStream = new();
                using CryptoStream cryptoStream = new(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
                cryptoStream.Write(bytes, 0, bytes.Length);
                return Convert.ToBase64String(memoryStream.ToArray());
            }

            private static string OPIPGDGHLCP(string input, int count)
            {
                StringBuilder stringBuilder = new(input);
                StringBuilder result = new(input.Length);
                for (int i = 0; i < input.Length; i++)
                {
                    char c = stringBuilder[i];
                    c = (char)(c ^ count);
                    _ = result.Append(c);
                }
                return result.ToString();
            }
        }

        private sealed class RootModel
        {
            public RootModel()
            {
                RemovePrefabs = new List<PD>();
                AddPrefabs = new List<PA>();
                AddRE = new List<RE>();
                AllPrefabs = new List<PA>();
                AddPathData = new List<PathDataModel>();
                DownloadUrl = string.Empty;
                Size = 4000;
            }

            public List<PD> RemovePrefabs { get; set; }
            public List<PA> AddPrefabs { get; set; }
            public List<RE> AddRE { get; set; }
            public List<PA> AllPrefabs { get; set; }
            public List<PathDataModel> AddPathData { get; set; }
            public uint Size { get; set; }
            public string DownloadUrl { get; set; }
        }

        public sealed class RE
        {
            public required string H;
            public required byte[] D;
            public int C;
        }

        public sealed class PD
        {
            public uint ID;
            public Vector3 P;
        }

        public sealed class PA
        {
            public uint ID;
            public required string C;
            public required string P;
            public required string R;
            public required string S;
        }

        internal sealed class PathDataModel
        {
            public required string Name { get; set; }
            public int Hierarchy { get; set; }
        }
    }
}
";
    }
}
