using MapUnlock.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MapUnlock.Core
{
    internal class PageManager : Sigleton<PageManager>, INotifyPropertyChanged
    {
        private object _currentPage;

        internal object CurrentPage 
        {
            get => _currentPage; 
            private set 
            {
                _currentPage = value;
                OnPropertyChanged(nameof(CurrentPage));
            }
        }

        internal Action<object> OnPageChanged { get; set; } = delegate { };

        internal void OpenMapSettingView()
        {
            CurrentPage = new MapSettingView();
            OnPageChanged.Invoke(CurrentPage);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
