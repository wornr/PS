using GalaSoft.MvvmLight.Command;

namespace PS.ViewModel.Windows {
    public sealed class MainViewModel : BaseViewModel {
        private RelayCommand _openLab1Page;
        private RelayCommand _openLab2Page;
        private RelayCommand _openLab3Page;
        private RelayCommand _openLab4Page;
        private RelayCommand _openLab5Page;

        public MainViewModel() {
            NavigateTo(ViewModelLocator.Instance.Lab1);
        }

        public RelayCommand OpenLab1Page => _openLab1Page ?? (_openLab1Page = new RelayCommand(() => NavigateTo(ViewModelLocator.Instance.Lab1)));
        public RelayCommand OpenLab2Page => _openLab2Page ?? (_openLab2Page = new RelayCommand(() => NavigateTo(ViewModelLocator.Instance.Lab2)));
        public RelayCommand OpenLab3Page => _openLab3Page ?? (_openLab3Page = new RelayCommand(() => NavigateTo(ViewModelLocator.Instance.Lab3)));
        public RelayCommand OpenLab4Page => _openLab4Page ?? (_openLab4Page = new RelayCommand(() => NavigateTo(ViewModelLocator.Instance.Lab4)));
        public RelayCommand OpenLab5Page => _openLab5Page ?? (_openLab5Page = new RelayCommand(() => NavigateTo(ViewModelLocator.Instance.Lab5)));
    }
}