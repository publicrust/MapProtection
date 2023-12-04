using Ionic.Zlib;
using MapUnlock.Core;
using MapUnlock.Extension;
using MapUnlock.Models;
using Newtonsoft.Json;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static WorldSerialization;

namespace MapUnlock.ViewModels
{
    internal class MapSettingViewModel : ViewModelBase
    {
        private WorldSerialization _worldSerialization = new WorldSerialization();
        private string _path;
        private Random _rnd = new Random();
        private int _size = 0;
        private readonly PrefabEntity _prefabEntity = new PrefabEntity();
        private readonly RustPlugin _rustPlugin = new RustPlugin();
        private List<RE> _addRE = new List<RE>();
        private List<PA> _addPrefabs = new List<PA>();
        private List<PD> _deletePrefabs = new List<PD>();

        private bool _isAddProtectionEnabled = true;
        private string _spamAmount = "5000";
        private bool _isREProtectChecked = true;
        private bool _isDeployProtectChecked = true;
        private bool _isEditProtectChecked = true;

        public string MapFile
        {
            get { return _path; }
            set
            {
                _path = value;
                OnPropertyChanged("MapFile");
            }
        }

        public bool IsAddProtectionEnabled
        {
            get { return _isAddProtectionEnabled; }
            set
            {
                _isAddProtectionEnabled = value;
                OnPropertyChanged("IsAddProtectionEnabled");
            }
        }

        public string SpamAmount
        {
            get { return _spamAmount; }
            set
            {
                _spamAmount = value;
                OnPropertyChanged("SpamAmount");
            }
        }

        public bool IsREProtectChecked
        {
            get { return _isREProtectChecked; }
            set
            {
                _isREProtectChecked = value;
                OnPropertyChanged("IsREProtectChecked");
            }
        }

        public bool IsDeployProtectChecked
        {
            get { return _isDeployProtectChecked; }
            set
            {
                _isDeployProtectChecked = value;
                OnPropertyChanged("IsDeployProtectChecked");
            }
        }

        public bool IsEditProtectChecked
        {
            get { return _isEditProtectChecked; }
            set
            {
                _isEditProtectChecked = value;
                OnPropertyChanged("IsEditProtectChecked");
            }
        }

        public ICommand SaveMapCommand { get; }
        public ICommand SelectMapCommand { get; }

        public ICommand OpenGithubCommand { get; }
        public ICommand OpenDiscordCommand { get; }

        public MapSettingViewModel()
        {
            SaveMapCommand = new RelayCommand(SaveMapCommandExecute, (s) => !string.IsNullOrWhiteSpace(MapFile));
            SelectMapCommand = new RelayCommand(SelectMapCommandExecute);
            OpenDiscordCommand = new RelayCommand(OpenDiscordCommandExecute);
            OpenGithubCommand = new RelayCommand(OpenGithubCommandExecute);
        }

        private void SelectMapCommandExecute(object obj)
        {
            _addRE = new List<RE>();
            _addPrefabs = new List<PA>();
            _deletePrefabs = new List<PD>();

            MapFile = GetPathOpenFileDialog();

            if (string.IsNullOrEmpty(MapFile))
            {
                MessageBox.Show("Need choose path");
                return;
            }
        }

        private void SaveMapCommandExecute(object obj)
        {
            if (string.IsNullOrWhiteSpace(MapFile))
            {
                MessageBox.Show("Need select map");
                return;
            }

            LoadWorldData();
            ProcessProtection();
            ProcessPrefabs();
            ProcessPumpJackOverflow();
            ProcessMapDataOverflow();
            ProcessSpamPrefabs();
            ShufflePrefabList();
            PatchAndSavePluginFile();
        }

        private void OpenDiscordCommandExecute(object obj)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://discord.gg/rFQ5tuEJvz",
                UseShellExecute = true
            });
        }

        private void OpenGithubCommandExecute(object obj)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/publicrust/MapProtection",
                UseShellExecute = true
            });
        }

        private void LoadWorldData()
        {
            _worldSerialization.Load(_path);
            _size = (int)_worldSerialization.world.size;
        }

        private void ProcessProtection()
        {
            //Remove RustEditData (Maps started from client folder will have no rust edit extention data)
            for (int i = _worldSerialization.world.maps.Count - 1; i >= 0; i--)
            {
                if (System.Text.Encoding.Default.GetString(_worldSerialization.world.maps[i].data).StartsWith("<?xml version")) 
                { 
                    _addRE.Add(
                        new RE().New(
                            _worldSerialization.world.maps[i].name, 
                            _worldSerialization.world.maps[i].data, 
                            _worldSerialization.world.prefabs.Count())
                        );

                    _worldSerialization.world.maps.RemoveAt(i); }
            }
        }

        private void ProcessPrefabs()
        {
            for (int i = _worldSerialization.world.prefabs.Count - 1; i >= 0; i--)
            {
                if (_prefabEntity.IsEntity(_worldSerialization.world.prefabs[i].id))
                {
                    PrefabData p = _worldSerialization.world.prefabs[i];
                    _addPrefabs.Add(new PA().New(p.id, p.category, p.position, p.rotation, p.scale));
                    _worldSerialization.world.prefabs.RemoveAt(i);
                    continue;
                }

                if (_worldSerialization.world.prefabs[i].id != 1724395471)
                {
                    _worldSerialization.world.prefabs[i].category = $":\\\\test black:{_rnd.Next(0, Math.Min(_worldSerialization.world.prefabs.Count, 40))}:";
                }
            }
        }

        private void ProcessPumpJackOverflow()
        {
            var pd = CreatePrefab(1599225199, new VectorData(), new VectorData(), new string('@', 200000000));
            _deletePrefabs.Add(new PD().New(pd.id, pd.position));
            _worldSerialization.world.prefabs.Add(pd);
        }

        private void ProcessMapDataOverflow()
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

        private void ProcessSpamPrefabs()
        {
            if (int.TryParse(SpamAmount.Split(" ").Last(), out int spam))
            {
                for (int i = 0; i < spam; i++)
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

                    PrefabData p = CreatePrefab(_prefabEntity.Esnts[_rnd.Next(0, _prefabEntity.Esnts.Count() - 1)], position, rotation);
                    _deletePrefabs.Add(new PD().New(p.id, p.position));
                    _worldSerialization.world.prefabs.Add(p);
                }
            }
        }

        private void ShufflePrefabList()
        {
            _worldSerialization.world.prefabs = ShufflePrefabs(_worldSerialization.world.prefabs);
        }

        private void PatchAndSavePluginFile()
        {
            string pluginFilePath = Path.Combine(Path.GetDirectoryName(_path), "MapProtection.cs");
            string pluginContent = _rustPlugin.Plugin
                .Replace("%SIZE%", $"{_size}")
                .Replace("%ADDKEY%", Convert.ToBase64String(Compression.Compress(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_addPrefabs)))))
                .Replace("%REKEY%", Convert.ToBase64String(Compression.Compress(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_addRE)))))
                .Replace("%PREFABKEY%", Convert.ToBase64String(Compression.Compress(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_deletePrefabs)))))
                .Replace("\"", "\"\"")
                .Replace(@"""""", @"""")
                ;

            File.WriteAllText(pluginFilePath, pluginContent);
            _worldSerialization.UpdatePassword();
            _worldSerialization.Save(_path + "protection.map");
            MessageBox.Show($"Save for path {_path + "protection.map"}");
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

        private string GetPathOpenFileDialog()
        {
            VistaOpenFileDialog openFileDialog = new VistaOpenFileDialog();
            openFileDialog.Filter = "Карты раст (*.map)|*.map";

            if (openFileDialog.ShowDialog() == false)
            {
                return "";
            }

            return openFileDialog.FileName;
        }
    }
}
