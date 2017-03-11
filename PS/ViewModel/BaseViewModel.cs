using GalaSoft.MvvmLight;
using PS.ViewModel.Pages;

namespace PS.ViewModel
{
    public abstract class BaseViewModel : ViewModelBase
    {
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
            //var dsManagerViewModel = viewModel as BaseViewModel;
            //dsManagerViewModel?.OnLoad();
        }
    }
}