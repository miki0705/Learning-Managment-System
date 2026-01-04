using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Learning_Management_System.Models;

namespace Learning_Management_System.Services
{
    public interface ILessonService
    {
        Task<IEnumerable<Lesson>> GetLessonsForDateRangeAsync(DateTime start, DateTime end);
        Task AddLessonAsync(Lesson lesson);
        Task UpdateLessonAsync(Lesson lesson);
        Task DeleteLessonAsync(int lessonId);

        // Metody wspierające Inteligentną Synchronizację (Opcja B)
        IEnumerable<Lesson> GetFutureLessonsBySchedule(int scheduleId);
        bool LessonExists(int groupId, int scheduleId, DateTime startTime);
    }
}