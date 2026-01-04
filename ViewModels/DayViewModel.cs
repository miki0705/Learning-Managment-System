using System;
using System.Collections.ObjectModel;
using Learning_Management_System.Models;
using Learning_Management_System.Helpers;

namespace Learning_Management_System.ViewModels
{
    public class DayViewModel : ViewModelBase
    {
        private DateTime _date;
        public DateTime Date
        {
            get => _date;
            set
            {
                _date = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DayName));
                OnPropertyChanged(nameof(FormattedDate));
            }
        }

        // Zwraca nazwę dnia (np. Poniedziałek)
        public string DayName => Date.ToString("dddd");

        // Zwraca datę w formacie (np. 05.01)
        public string FormattedDate => Date.ToString("dd.MM");

        // Kolekcja lekcji przypisanych do tego konkretnego dnia
        public ObservableCollection<Lesson> Lessons { get; set; } = new ObservableCollection<Lesson>();
    }
}