using MapUnlock.Core;
using MapUnlock.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MapUnlock.ViewModels
{
    class MainViewModel : ViewModelBase
    {
        private object _currentPage;
        public object CurrentPage 
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                OnPropertyChanged(nameof(CurrentPage));
            }
        }

        public MainViewModel()
        {
            new PageManager();

            PageManager.Instance.OnPageChanged += OnPageChanged;

            PageManager.Instance.OpenMapSettingView();
        }

        private void OnPageChanged(object obj)
        {
            CurrentPage = obj;
        }
    }
}
