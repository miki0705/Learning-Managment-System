using System;
using System.Globalization;
using System.Windows.Data;

namespace Learning_Management_System.Helpers
{
    /// <summary>
    /// Converter for binding decimal? to TextBox (handles null values)
    /// </summary>
    public class DecimalConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal decimalValue)
                return decimalValue.ToString(culture);
            
            // Handle nullable decimal
            var nullableDecimal = value as decimal?;
            if (nullableDecimal.HasValue)
                return nullableDecimal.Value.ToString(culture);
            
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue && !string.IsNullOrWhiteSpace(stringValue))
            {
                if (decimal.TryParse(stringValue, NumberStyles.Any, culture, out decimal result))
                {
                    if (targetType == typeof(decimal?))
                        return (decimal?)result;
                    return result;
                }
            }
            
            // Return null for nullable types, or 0 for non-nullable
            if (targetType == typeof(decimal?))
                return null!; // Explicitly return null for nullable decimal
            
            return 0m;
        }
    }
}
