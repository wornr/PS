using System.Windows;
using System.Windows.Controls;
using PS.Model;
using PS.ViewModel;

namespace PS.View.Pages {
    public partial class Lab4 {
        public Lab4() {
            InitializeComponent();
        }

        private void TreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            ViewModelLocator.Instance.Lab4.ChangeDir = ((Dir) ((TreeView) sender)?.SelectedItem)?.Name;
        }
    }
}