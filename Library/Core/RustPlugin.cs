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
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using Aes = System.Security.Cryptography.Aes;
using System.Text;
using CompanionServer.Handlers;
using Facepunch.Utility;
using HarmonyLib;
using Newtonsoft.Json;
using ProtoBuf;
using EndOfStreamException = System.IO.EndOfStreamException;
using MemoryStream = System.IO.MemoryStream;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info(""MapProtection[%MapName%]"", ""bmgjet & FREE RUST"", ""2.0.4"")]
    [Description(""https://github.com/publicrust/MapProtection"")]
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

        private static void SetUrl(string url)
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
                _root = new Lazy<RootModel>(
                    () =>
                        JsonConvert.DeserializeObject<RootModel>(
                            Encoding.UTF8.GetString(
                                Compression.Uncompress(Convert.FromBase64String(Key))
                            )
                        )
                );
            }

            if (
                _root?.Value.DownloadUrl is not null
                && !string.IsNullOrWhiteSpace(_root.Value.DownloadUrl)
            )
            {
                SetUrl(_root.Value.DownloadUrl);
            }

            _harmony = new Harmony(Name + ""PATCH"");
            _harmony.PatchAll();
        }

        private void OnTerrainCreate(TerrainGenerator terrainGenerator)
        {
            if (_root != null)
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
            _harmony?.UnpatchAll(Name + ""PATCH"");
            SetPlugin(null);
        }

        public VectorData String2Vector(string vectorData)
        {
            string[] s = vectorData.Split(separator, StringSplitOptions.None);
            return new VectorData(
                float.Parse(s[0], CultureInfo.InvariantCulture),
                float.Parse(s[1], CultureInfo.InvariantCulture),
                float.Parse(s[2], CultureInfo.InvariantCulture)
            );
        }

        public VectorData StringToVectorData(string vectorData)
        {
            string[] s = vectorData.Split(separator, StringSplitOptions.None);
            if (s.Length >= 3)
            {
                float x = float.Parse(s[0], CultureInfo.InvariantCulture);
                float y = float.Parse(s[1], CultureInfo.InvariantCulture);
                float z = float.Parse(s[2], CultureInfo.InvariantCulture);
                return new VectorData(x, y, z);
            }
            Debug.LogWarning(""StringToVectorData: Invalid input format.Returning Vector3.zero."");

            return new VectorData(0, 0, 0);
        }

        [HarmonyPatch(
            typeof(MapData),
            nameof(MapData.DeserializeLengthDelimited),
            typeof(BufferStream),
            typeof(MapData),
            typeof(bool)
        )]
        internal static class DeserializeLengthDelimitedHook
        {
            private static bool Prefix(
                BufferStream stream,
                MapData instance,
                bool isDelta,
                ref MapData __result
            )
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
                            throw new ProtocolBufferException(
                                ""Invalid field id: 0, something went wrong in the stream""
                            );
                        }

                        ProtocolParser.SkipKey(stream, key);
                    }
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
        internal static class OnWorldLoadHook
        {
            [HarmonyPostfix]
            private static void Postfix(WorldSerialization __instance)
            {
                if (plugin?._root == null)
                {
                    return;
                }

                MapData mapData = GetPasswordMap(__instance);

                if (mapData != null)
                {
                    mapData.data = Array.Empty<byte>();
                }

                MapData hieght = __instance.GetMap(""hieght"");

                if (hieght != null)
                {
                    hieght.data = Array.Empty<byte>();
                }

                for (int i = 0; i < __instance.world.prefabs.Count; i++)
                {
                    if (__instance.world.prefabs[i].id == 1724395471)
                    {
                        continue;
                    }

                    __instance.world.prefabs[i].category = ""Decor"";
                }

                int D = 0;
                int A = 0;

                if (plugin._root.Value.RemovePrefabs?.Count > 0)
                {
                    for (int i = __instance.world.prefabs.Count - 1; i >= 0; i--)
                    {
                        for (int p = plugin._root.Value.RemovePrefabs.Count - 1; p >= 0; p--)
                        {
                            try
                            {
                                if (
                                    plugin._root.Value.RemovePrefabs[p].ID
                                        == __instance.world.prefabs[i].id
                                    && plugin._root.Value.RemovePrefabs[p].P
                                        == __instance.world.prefabs[i].position
                                )
                                {
                                    __instance.world.prefabs.RemoveAt(i);
                                    plugin._root.Value.RemovePrefabs.RemoveAt(p);
                                    D++;
                                    break;
                                }
                            }
                            catch (Exception ex)
                            {
                                plugin.Puts($""Error removing prefab: {ex}"");
                            }
                        }
                    }
                }

                if (plugin._root.Value.AddPathData?.Count > 0)
                {
                    foreach (PathDataModel p in plugin._root.Value.AddPathData)
                    {
                        PathData pathData = __instance.world.paths.FirstOrDefault(s =>
                            s.name == p.Name
                        );
                        if (pathData == null)
                        {
                            plugin.Puts(""path not found "" + p.Name);
                            continue;
                        }

                        pathData.hierarchy = p.Hierarchy;
                    }
                }

                if (plugin._root.Value.AddPrefabs?.Count > 0)
                {
                    foreach (PA p in plugin._root.Value.AddPrefabs)
                    {
                        PrefabData pd = new()
                        {
                            id = p.ID,
                            category = p.C,
                            position = plugin.String2Vector(p.P),
                            rotation = plugin.String2Vector(p.R),
                            scale = plugin.String2Vector(p.S),
                        };
                        __instance.world.prefabs.Add(pd);
                        A++;
                    }
                }

                if (plugin._root.Value.AllPrefabs?.Count > 0)
                {
                    foreach (PA p in plugin._root.Value.AllPrefabs)
                    {
                        PrefabData prefab = __instance.world.prefabs.FirstOrDefault(s =>
                            s.id == p.ID
                            && s.position == plugin.StringToVectorData(p.P)
                            && s.rotation == plugin.StringToVectorData(p.R)
                            && s.scale == plugin.StringToVectorData(p.S)
                        );

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
                    foreach (RE p in plugin._root.Value.AddRE)
                    {
                        if (p.C != PC)
                        {
                            plugin.Puts(""Invalid Prefab Count "" + p.C + "" / "" + PC);
                            break;
                        }
                        __instance.AddMap(p.H, p.D);
                    }
                }

                Debug.LogWarning(
                    ""[Map Protecion] Removed ""
                        + D
                        + "" Spam Prefabs / Restored ""
                        + A
                        + "" Missing Prefabs""
                );
            }

            private static MapData GetPasswordMap(WorldSerialization worldSerialization)
            {
                int prefabsCount = worldSerialization.world.prefabs.Count;
                return worldSerialization.world.maps.FirstOrDefault(s =>
                    s.name == OPIPGDGHLCP(""mappassword"", prefabsCount)
                    || s.name == EIGDJDKLLED(""mappassword"", prefabsCount)
                );
            }

            private static string EIGDJDKLLED(string KBJLCHGLCPN, int FNCNKFNPJAB)
            {
                string password = FNCNKFNPJAB.ToString(CultureInfo.InvariantCulture);
                byte[] bytes = Encoding.Unicode.GetBytes(KBJLCHGLCPN);
                using (Aes aes = Aes.Create())
                {
                    Rfc2898DeriveBytes rfc2898DeriveBytes = new(
                        password,
                        ""Ivan Medvedev""u8.ToArray(),
                        100000,
                        HashAlgorithmName.SHA512
                    );
                    aes.Key = rfc2898DeriveBytes.GetBytes(32);
                    aes.IV = rfc2898DeriveBytes.GetBytes(16);
                    using MemoryStream memoryStream = new();
                    using (
                        CryptoStream cryptoStream = new(
                            memoryStream,
                            aes.CreateEncryptor(),
                            CryptoStreamMode.Write
                        )
                    )
                    {
                        cryptoStream.Write(bytes, 0, bytes.Length);
                    }
                    KBJLCHGLCPN = Convert.ToBase64String(memoryStream.ToArray());
                }
                return KBJLCHGLCPN;
            }

            private static string OPIPGDGHLCP(string JKNFEDNIELG, int FNCNKFNPJAB)
            {
                StringBuilder stringBuilder = new(JKNFEDNIELG);
                StringBuilder stringBuilder2 = new(JKNFEDNIELG.Length);
                for (int i = 0; i < JKNFEDNIELG.Length; i++)
                {
                    char c = stringBuilder[i];
                    c = (char)(c ^ FNCNKFNPJAB);
                    _ = stringBuilder2.Append(c);
                }
                return stringBuilder2.ToString();
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
                Size = 0;
                DownloadUrl = string.Empty;
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
            public required string H { get; set; }
            public required byte[] D { get; set; }
            public int C { get; set; }
        }

        public sealed class PD
        {
            public uint ID;
            public VectorData P;
        }

        public sealed class PA
        {
            public uint ID { get; set; }
            public required string C { get; set; }
            public required string P { get; set; }
            public required string R { get; set; }
            public required string S { get; set; }
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