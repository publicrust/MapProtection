using Ionic.Zlib;
using Library;
using MapProtection.Core;
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

namespace MapUnlock.ViewModels
{
    internal class MapSettingViewModel : ViewModelBase
    {
        private string _path;
        
        private bool _isAddProtectionEnabled = true;
        private string _spamAmount = "5000";
        private bool _isREProtectChecked = true;
        private bool _isDeployProtectChecked = true;
        private bool _isEditProtectChecked = true;
        private bool _isUploadMap = true;

        public bool IsUploadMap
        {
            get { return _isUploadMap; }
            set
            {
                _isUploadMap = value;
                OnPropertyChanged("IsUploadMap");
            }
        }

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
            MapFile = GetPathOpenFileDialog();

            if (string.IsNullOrEmpty(MapFile))
            {
                MessageBox.Show("Need choose path");
            }
        }

        private void SaveMapCommandExecute(object obj)
        {
            if (string.IsNullOrWhiteSpace(MapFile))
            {
                MessageBox.Show("Need select map");
                return;
            }

            if(!int.TryParse(SpamAmount, out int spamAmount))
                spamAmount = 0;

            Map _map = new Map();

            var result = _map.Protect(_path, new Library.Models.MapProtectOptions()
            {
                SpamAmount = spamAmount,
                IsDeployProtectChecked = IsDeployProtectChecked,
                IsEditProtectChecked = IsEditProtectChecked,
                IsUploadMap = IsUploadMap,
                IsRustEditDataProtectChecked = IsREProtectChecked,
            });

            string pluginFilePath = Path.Combine(Path.GetDirectoryName(_path), "MapProtection.cs");

            File.WriteAllText(pluginFilePath, result.Plugin);

            if (IsUploadMap)
            {
                MessageBox.Show($"Save for path {pluginFilePath}");
            }
            else
            {
                result.Map.Save(_path + "protection.map");
                MessageBox.Show($"Save for path {_path + "protection.map"}");
            }
        }

        private void OpenDiscordCommandExecute(object obj)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://discord.gg/Qr59w6YRk6",
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
