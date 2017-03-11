using GalaSoft.MvvmLight.Ioc;

using Microsoft.Practices.ServiceLocation;

using PS.ViewModel.Pages;
using PS.ViewModel.Windows;

namespace PS.ViewModel {
    public class ViewModelLocator {
        private static ViewModelLocator _instance;

        public static ViewModelLocator Instance => _instance ?? (_instance = new ViewModelLocator());

        public ViewModelLocator() {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<MainViewModel>();

            SimpleIoc.Default.Register<Lab1ViewModel>();
            SimpleIoc.Default.Register<Lab2ViewModel>();
            SimpleIoc.Default.Register<Lab3ViewModel>();
            SimpleIoc.Default.Register<Lab4ViewModel>();
            SimpleIoc.Default.Register<Lab5ViewModel>();
        }

        public MainViewModel Main => ServiceLocator.Current.GetInstance<MainViewModel>();

        public Lab1ViewModel Lab1 => ServiceLocator.Current.GetInstance<Lab1ViewModel>();
        public Lab2ViewModel Lab2 => ServiceLocator.Current.GetInstance<Lab2ViewModel>();
        public Lab3ViewModel Lab3 => ServiceLocator.Current.GetInstance<Lab3ViewModel>();
        public Lab4ViewModel Lab4 => ServiceLocator.Current.GetInstance<Lab4ViewModel>();
        public Lab5ViewModel Lab5 => ServiceLocator.Current.GetInstance<Lab5ViewModel>();

        public static void Cleanup()
        {
            SimpleIoc.Default.Unregister<MainViewModel>();

            SimpleIoc.Default.Unregister<Lab1ViewModel>();
            SimpleIoc.Default.Unregister<Lab2ViewModel>();
            SimpleIoc.Default.Unregister<Lab3ViewModel>();
            SimpleIoc.Default.Unregister<Lab4ViewModel>();
            SimpleIoc.Default.Unregister<Lab5ViewModel>();
        }
    }
}