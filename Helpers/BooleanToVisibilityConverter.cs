using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Learning_Management_System.Helpers
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool bValue = value is bool b && b;

            // Jeśli w parametrze jest "Inverted", odwracamy logiczną wartość
            if (parameter?.ToString() == "Inverted")
                bValue = !bValue;

            // KLUCZ: Jeśli bindowanie idzie do IsEnabled (które jest bool), zwracamy bool
            if (targetType == typeof(bool))
                return bValue;

            // Jeśli bindowanie idzie do Visibility, zwracamy Visible/Collapsed
            return bValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility v && v == Visibility.Visible;
        }
    }
}