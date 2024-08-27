using Library.Core;
using Library.Extensions;
using Library.Models;
using Library.Utils;
using Newtonsoft.Json;
using System.Text;

namespace Library
{
    public class Map
    {
        private readonly WorldSerialization _worldSerialization = new WorldSerialization();
        private string _path;
        private readonly Random _rnd = new Random();
        private RootModel _root = new RootModel();

        private int _startPrefabsCount;
        private MapProtectOptions _options;

        public ResultModel Protect(string path, MapProtectOptions options)
        {
            _root.AddRE = new List<RE>();
            _root.AddPrefabs = new List<PA>();
            _root.RemovePrefabs = new List<PD>();
            _root.AllPrefabs = new List<PA>();
            _root.AddPathData = new List<PathDataModel>();

            _path = path;
            _options = options;

            LoadWorldData();

            foreach(var prefab in _worldSerialization.world.prefabs)
            {
                _root.AllPrefabs.Add(new PA().New(prefab.id, prefab.category, prefab.position, prefab.rotation, prefab.scale));
            }

            PathProtect();
            ProcessProtection();
            ProcessPrefabs();
            ProcessPumpJackOverflow();
            ProcessMapDataOverflow();
            ProcessSpamPrefabs();

            _worldSerialization.world.prefabs = _worldSerialization.world.prefabs.ShufflePrefabs();

            if (options.IsUploadMap)
            {
                using (var stream = _worldSerialization.SaveToStream())
                {
                    _root.DownloadUrl = MapUploader.UploadMap(stream, Path.GetFileName(path));

                    if (string.IsNullOrWhiteSpace(_root.DownloadUrl))
                    {
                        throw new Exception("Failed to upload map.");
                    }
                }
            }

            string pluginContent = RustPlugin.Plugin
                .Replace("%ROOT%", Convert.ToBase64String(Ionic.Zlib.GZipStream.CompressBuffer(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_root)))))
                .Replace("\"", "\"\"")
                .Replace(@"""""", @"""")
                ;

            _worldSerialization.UpdatePassword();

            pluginContent = pluginContent.Replace("%MAPPASSWORD%", _worldSerialization.RetrievePasswordMap().name);

            return new ResultModel()
            {
                Map = _worldSerialization,
                Plugin = pluginContent,
            };

        }

        private void LoadWorldData()
        {
            _worldSerialization.Load(_path);

            _startPrefabsCount = _worldSerialization.world.prefabs.Count;

            _root.Size = _worldSerialization.world.size;
        }

        private void PathProtect()
        {
            if (!_options.IsRustEditDataProtectChecked)
                return;

            foreach (var pathData in _worldSerialization.world.paths)
            {
                _root.AddPathData.Add(new PathDataModel()
                {
                    Name = pathData.name,
                    Hierarchy = pathData.hierarchy
                });

                pathData.hierarchy = 0;
            }

        }

        private void ProcessProtection()
        {
            if (!_options.IsRustEditDataProtectChecked)
                return;

            //Remove RustEditData (Maps started from client folder will have no rust edit extention data)
            for (int i = _worldSerialization.world.maps.Count - 1; i >= 0; i--)
            {
                if (Encoding.Default.GetString(_worldSerialization.world.maps[i].data).StartsWith("<?xml version"))
                {
                    var blob = WorldManager.FindBlob(_worldSerialization.world.maps[i].name, _startPrefabsCount);

                    _root.AddRE.Add(
                        new RE().New(
                            string.IsNullOrWhiteSpace(blob)
                            ?
                                _worldSerialization.world.maps[i].name
                            :
                                WorldManager.Encrypt(blob, _worldSerialization.world.prefabs.Count),
                            _worldSerialization.world.maps[i].data,
                            _worldSerialization.world.prefabs.Count)
                        );

                    _worldSerialization.world.maps.RemoveAt(i);
                }
            }
        }

        private void ProcessPrefabs()
        {
            for (int i = _worldSerialization.world.prefabs.Count - 1; i >= 0; i--)
            {
                if (_options.IsDeployProtectChecked)
                {
                    if (_worldSerialization.world.prefabs[i].IsEntity())
                    {
                        PrefabData p = _worldSerialization.world.prefabs[i];
                        _root.AddPrefabs.Add(new PA().New(p.id, p.category, p.position, p.rotation, p.scale));
                        _worldSerialization.world.prefabs.RemoveAt(i);
                        continue;
                    }
                }

                if (_options.IsEditProtectChecked)
                {
                    if (_worldSerialization.world.prefabs[i].id != 1724395471)
                    {
                        _worldSerialization.world.prefabs[i].category = $":\\\\test black:{_rnd.Next(0, Math.Min(_worldSerialization.world.prefabs.Count, 40))}:";
                    }
                }
            }
        }

        private void ProcessPumpJackOverflow()
        {
            if (_options.IsEditProtectChecked)
            {
                var pd = CreatePrefab(1599225199, new VectorData(), new VectorData(), new string('@', 200000000));
                _root.RemovePrefabs.Add(new PD().New(pd.id, pd.position));
                _worldSerialization.world.prefabs.Add(pd);
            }
        }

        private void ProcessMapDataOverflow()
        {
            if (_options.IsEditProtectChecked)
            {
                if (_worldSerialization.GetMap("hieght") == null)
                {
                    _worldSerialization.AddMap("hieght", new byte[200000000]);
                }

                var pd = CreatePrefab(1237378647, new VectorData(), new VectorData(), $":\\test black:1:");
                _root.RemovePrefabs.Add(new PD().New(pd.id, pd.position));
                _worldSerialization.world.prefabs.Add(pd);
                _worldSerialization.world.size = (uint)_rnd.Next(111111111, int.MaxValue);
            }
        }

        private void ProcessSpamPrefabs()
        {
            if(_options.SpamAmount == null)
                return;

            for (int i = 0; i < _options.SpamAmount; i++)
            {
                VectorData position;
                VectorData rotation;

                if (_worldSerialization.world.prefabs.Count > i)
                {
                    position = _worldSerialization.world.prefabs[i].position;
                    rotation = _worldSerialization.world.prefabs[i].rotation;
                }
                else
                {
                    position = new VectorData(
                        _rnd.Next((int)(_root.Size / 3 * -1), (int)(_root.Size / 3)),
                        _rnd.Next(-60, 300),
                        _rnd.Next((int)(_root.Size / 3 * -1), (int)(_root.Size / 3))
                    );
                    rotation = new VectorData(_rnd.Next(0, 359), _rnd.Next(0, 359), _rnd.Next(0, 359));
                }

                PrefabData p = CreatePrefab(PrefabDataExtensions.Entitys[_rnd.Next(0, PrefabDataExtensions.Entitys.Count - 1)], position, rotation);
                _root.RemovePrefabs.Add(new PD().New(p.id, p.position));
                _worldSerialization.world.prefabs.Add(p);
            }
        }

        private PrefabData CreatePrefab(uint PrefabID, VectorData posistion, VectorData rotation, string category = ":\\test black:1:")
        {
            var prefab = new PrefabData()
            {
                category = category,
                id = PrefabID,
                position = posistion,
                rotation = rotation,
                scale = new VectorData(1, 1, 1)
            };

            return prefab;
        }
    }
}
