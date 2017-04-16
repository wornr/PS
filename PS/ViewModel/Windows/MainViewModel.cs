using MahApps.Metro.Controls;

namespace PS.ViewModel.Windows {
    public sealed class MainViewModel : BaseViewModel {
        private HamburgerMenuGlyphItem _selectedMenuItem;
        private HamburgerMenuItemCollection _menuItems;

        private readonly HamburgerMenuGlyphItem _lab1 = new HamburgerMenuGlyphItem
        {
            Glyph = "",
            Label = "Lab1 - base64",
            Tag = ViewModelLocator.Instance.Lab1
        };
        private readonly HamburgerMenuGlyphItem _lab2 = new HamburgerMenuGlyphItem
        {
            Glyph = "",
            Label = "Lab2 - POP3",
            Tag = ViewModelLocator.Instance.Lab2
        };
        private readonly HamburgerMenuGlyphItem _lab3 = new HamburgerMenuGlyphItem
        {
            Glyph = "",
            Label = "Lab3 - SMTP",
            Tag = ViewModelLocator.Instance.Lab3
        };
        private readonly HamburgerMenuGlyphItem _lab4 = new HamburgerMenuGlyphItem
        {
            Glyph = "",
            Label = "Lab4 - FTP",
            Tag = ViewModelLocator.Instance.Lab4
        };
        private readonly HamburgerMenuGlyphItem _lab5 = new HamburgerMenuGlyphItem
        {
            Glyph = "",
            Label = "Lab5 - HTTP Crawler",
            Tag = ViewModelLocator.Instance.Lab5
        };


        public MainViewModel() {
            MenuItems = new HamburgerMenuItemCollection {
                _lab1,
                _lab2,
                _lab3,
                _lab4,
                _lab5
            };
            SelectedMenuItem = _lab1; // Default view after app run
        }

        public HamburgerMenuGlyphItem SelectedMenuItem {
            get => _selectedMenuItem;
            set {
                if (Equals(_selectedMenuItem, value))
                    return;

                _selectedMenuItem = value;
                RaisePropertyChanged();
            }
        }
        public HamburgerMenuItemCollection MenuItems {
            get => _menuItems;
            set {
                if (Equals(_menuItems, value))
                    return;

                _menuItems = value;
                RaisePropertyChanged();
            }
        }
    }
}