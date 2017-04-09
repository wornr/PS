using System;
using System.Globalization;
using System.Windows.Data;
using PS.Model;
using Type = System.Type;

namespace PS.Converters {
    public class IconConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null)
                return string.Empty;

            switch ((Model.Type)value) {
                case Model.Type.Directory:
                    return "/PS;component/Resources/dir.png";
                case Model.Type.File:
                    return "/PS;component/Resources/file.png";
                case Model.Type.Unknown:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}