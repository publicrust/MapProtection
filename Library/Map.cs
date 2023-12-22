using Library.Core;
using Library.Extensions;
using Library.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library
{
    public class Map
    {
        private WorldSerialization _worldSerialization = new WorldSerialization();
        private string _path;
        private Random _rnd = new Random();
        private int _size = 0;
        private List<RE> _addRE;
        private List<PA> _addPrefabs;
        private List<PD> _deletePrefabs;
        private int _startPrefabsCount;
        private MapProtectOptions _options;

        public void Protect(string path, MapProtectOptions options)
        {
            _addRE = new List<RE>();
            _addPrefabs = new List<PA>();
            _deletePrefabs = new List<PD>();

            _path = path;
            _options = options;

            LoadWorldData();
            ProcessProtection();
            ProcessPrefabs();
            ProcessPumpJackOverflow();
            ProcessMapDataOverflow();
            ProcessSpamPrefabs();
            ShufflePrefabList();
            PatchAndSavePluginFile();
        }

        private void LoadWorldData()
        {
            _worldSerialization.Load(_path);

            _startPrefabsCount = _worldSerialization.world.prefabs.Count;

            _size = (int)_worldSerialization.world.size;
        }

        private void ProcessProtection()
        {
            if (!_options.IsREProtectChecked)
                return;

            //Remove RustEditData (Maps started from client folder will have no rust edit extention data)
            for (int i = _worldSerialization.world.maps.Count - 1; i >= 0; i--)
            {
                if (Encoding.Default.GetString(_worldSerialization.world.maps[i].data).StartsWith("<?xml version"))
                {
                    var blob = WorldManager.FindBlob(_worldSerialization.world.maps[i].name, _startPrefabsCount);

                    _addRE.Add(
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
                        _addPrefabs.Add(new PA().New(p.id, p.category, p.position, p.rotation, p.scale));
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
                _deletePrefabs.Add(new PD().New(pd.id, pd.position));
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
                _deletePrefabs.Add(new PD().New(pd.id, pd.position));
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
                    position = new VectorData(_rnd.Next(_size / 3 * -1, _size / 3), _rnd.Next(-60, 300), _rnd.Next(_size / 3 * -1, _size / 3));
                    rotation = new VectorData(_rnd.Next(0, 359), _rnd.Next(0, 359), _rnd.Next(0, 359));
                }

                PrefabData p = CreatePrefab(PrefabDataExtensions.Entitys[_rnd.Next(0, PrefabDataExtensions.Entitys.Count() - 1)], position, rotation);
                _deletePrefabs.Add(new PD().New(p.id, p.position));
                _worldSerialization.world.prefabs.Add(p);
            }
        }

        private void ShufflePrefabList()
        {
            _worldSerialization.world.prefabs = ShufflePrefabs(_worldSerialization.world.prefabs);
        }

        private void PatchAndSavePluginFile()
        {
            string pluginFilePath = Path.Combine(Path.GetDirectoryName(_path), "MapProtection.cs");
            string pluginContent = RustPlugin.Plugin
                .Replace("%SIZE%", $"{_size}")
                .Replace("%ADDKEY%", Convert.ToBase64String(Ionic.Zlib.GZipStream.CompressBuffer(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_addPrefabs)))))
                .Replace("%REKEY%", Convert.ToBase64String(Ionic.Zlib.GZipStream.CompressBuffer(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_addRE)))))
                .Replace("%PREFABKEY%", Convert.ToBase64String(Ionic.Zlib.GZipStream.CompressBuffer(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_deletePrefabs)))))
                .Replace("\"", "\"\"")
                .Replace(@"""""", @"""")
                ;

            File.WriteAllText(pluginFilePath, pluginContent);
            _worldSerialization.UpdatePassword();
            _worldSerialization.Save(_path + "protection.map");
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

        private List<PrefabData> ShufflePrefabs(List<PrefabData> listToShuffle)
        {
            for (int i = listToShuffle.Count - 1; i > 0; i--)
            {
                int k = _rnd.Next(i + 1);
                var value = listToShuffle[k];
                listToShuffle[k] = listToShuffle[i];
                listToShuffle[i] = value;
            }
            return listToShuffle;
        }
    }
}
