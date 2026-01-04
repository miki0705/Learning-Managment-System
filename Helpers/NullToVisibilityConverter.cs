using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Learning_Management_System.Helpers
{
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Sprawdzamy tylko, czy obiekt nie jest nullem
            bool isVisible = value != null;

            // Jeśli dodaliśmy parametr "Inverted", odwracamy logikę
            if (parameter?.ToString() == "Inverted")
                isVisible = !isVisible;

            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Nie potrzebujemy konwersji zwrotnej dla widoczności
            throw new NotImplementedException();
        }
    }
}