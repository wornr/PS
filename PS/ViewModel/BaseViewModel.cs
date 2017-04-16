using GalaSoft.MvvmLight;

using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

using System.Windows;

namespace PS.ViewModel
{
    public abstract class BaseViewModel : ViewModelBase {
        private readonly MetroWindow _windowsInstance = Application.Current.MainWindow as MetroWindow;

        public async void DisplayDialog(string title, string description) {
            await _windowsInstance.ShowMessageAsync(title, description);
        }
    }
}