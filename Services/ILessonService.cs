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
        Task SyncLessonsWithScheduleAsync(Group group);
        Task<IEnumerable<Lesson>> GetFutureLessonsByScheduleAsync(int scheduleId); // Nazwa z Async
        Task<bool> LessonExistsAsync(int groupId, int scheduleId, DateTime startTime);
        Task CompleteLessonReportAsync(int lessonId, IPaymentService paymentService);
    }
}