using GalaSoft.MvvmLight;

using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

using System.Windows;

namespace PS.ViewModel
{
    public abstract class BaseViewModel : ViewModelBase {
        private readonly MetroWindow _windowsInstance = Application.Current.MainWindow as MetroWindow;
        private ViewModelBase _currentViewModel;

        public ViewModelBase CurrentViewModel {
            get { return _currentViewModel; }
            set {
                if (_currentViewModel == value)
                    return;
                _currentViewModel = value;
                RaisePropertyChanged();
            }
        }

        public virtual void NavigateTo(ViewModelBase viewModel) {
            CurrentViewModel = viewModel;
        }

        public async void DisplayDialog(string title, string description) {
            await _windowsInstance.ShowMessageAsync(title, description);
        }
    }
}