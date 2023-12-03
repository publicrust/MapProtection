using System;
using System.Windows;

namespace MapUnlock.Core
{
    internal class TheardForm
    {
        public static void Call(Action action)
        {
            if (Application.Current == null || Application.Current.Dispatcher.CheckAccess())
            {
                action.Invoke();
                return;
            }

            Application.Current.Dispatcher.Invoke(() => action.Invoke());
        }
    }
}
