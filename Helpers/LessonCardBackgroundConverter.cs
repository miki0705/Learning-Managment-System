using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Learning_Management_System.Models;

namespace Learning_Management_System.Helpers
{
    public class LessonCardBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is not Lesson lesson || lesson == null)
                    return Brushes.Transparent;

                // Light green for completed lessons
                if (lesson.Status == LessonStatus.Completed)
                {
                    return new SolidColorBrush(Color.FromArgb(255, 200, 255, 200)); // Light green
                }

                // Light orange for scheduled lessons with past dates
                if (lesson.Status == LessonStatus.Scheduled)
                {
                    try
                    {
                        if (lesson.StartTime.Date < DateTime.Today)
                        {
                            return new SolidColorBrush(Color.FromArgb(255, 255, 220, 177)); // Light orange
                        }
                    }
                    catch
                    {
                        // If StartTime is not initialized, return transparent
                        return Brushes.Transparent;
                    }
                }

                return Brushes.Transparent;
            }
            catch
            {
                // Return transparent on any error to prevent app crash
                return Brushes.Transparent;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
