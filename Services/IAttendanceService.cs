using Learning_Management_System.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Learning_Management_System.Services
{
    public interface IAttendanceService
    {
        Task<IEnumerable<Attendance>> GetAttendancesByLessonAsync(int lessonId);
        Task<IEnumerable<Attendance>> GetAttendancesByStudentAsync(int studentId);
        Task<IEnumerable<Attendance>> GetAttendancesByDateRangeAsync(DateTime start, DateTime end);
        Task AddOrUpdateAttendanceAsync(Attendance attendance);
        Task BulkUpdateAttendancesAsync(int lessonId, List<Attendance> attendances);
        Task DeleteAttendanceAsync(int attendanceId);
        Task<IEnumerable<Lesson>> GetLessonsWithoutAttendanceAsync();
    }
}
