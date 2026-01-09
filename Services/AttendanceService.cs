using Learning_Management_System.Data;
using Learning_Management_System.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Learning_Management_System.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly AppDbContext _db;

        public AttendanceService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Attendance>> GetAttendancesByLessonAsync(int lessonId)
        {
            return await _db.Attendances
                .Include(a => a.Student)
                .Include(a => a.Lesson)
                    .ThenInclude(l => l.Group)
                .Where(a => a.LessonId == lessonId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Attendance>> GetAttendancesByStudentAsync(int studentId)
        {
            return await _db.Attendances
                .Include(a => a.Lesson)
                    .ThenInclude(l => l.Group)
                .Where(a => a.StudentId == studentId)
                .OrderByDescending(a => a.Lesson.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Attendance>> GetAttendancesByDateRangeAsync(DateTime start, DateTime end)
        {
            return await _db.Attendances
                .Include(a => a.Student)
                .Include(a => a.Lesson)
                    .ThenInclude(l => l.Group)
                .Where(a => a.Lesson.StartTime >= start && a.Lesson.StartTime <= end)
                .OrderByDescending(a => a.Lesson.StartTime)
                .ToListAsync();
        }

        public async Task AddOrUpdateAttendanceAsync(Attendance attendance)
        {
            var existing = await _db.Attendances
                .FirstOrDefaultAsync(a => a.LessonId == attendance.LessonId && 
                                          a.StudentId == attendance.StudentId);

            if (existing != null)
            {
                // Update existing
                existing.Status = attendance.Status;
                existing.PriceCharged = attendance.PriceCharged;
                _db.Entry(existing).State = EntityState.Modified;
            }
            else
            {
                // Add new
                _db.Attendances.Add(attendance);
            }

            await _db.SaveChangesAsync();
        }

        public async Task BulkUpdateAttendancesAsync(int lessonId, List<Attendance> attendances)
        {
            // Get existing attendances for this lesson
            var existingAttendances = await _db.Attendances
                .Where(a => a.LessonId == lessonId)
                .ToListAsync();

            // Remove attendances that are no longer in the list
            var attendanceStudentIds = attendances.Select(a => a.StudentId).ToList();
            var toRemove = existingAttendances
                .Where(e => !attendanceStudentIds.Contains(e.StudentId))
                .ToList();

            foreach (var attendance in toRemove)
            {
                _db.Attendances.Remove(attendance);
            }

            // Add or update attendances
            foreach (var attendance in attendances)
            {
                var existing = existingAttendances
                    .FirstOrDefault(e => e.StudentId == attendance.StudentId);

                if (existing != null)
                {
                    existing.Status = attendance.Status;
                    existing.PriceCharged = attendance.PriceCharged;
                }
                else
                {
                    attendance.LessonId = lessonId;
                    _db.Attendances.Add(attendance);
                }
            }

            await _db.SaveChangesAsync();
        }

        public async Task DeleteAttendanceAsync(int attendanceId)
        {
            var attendance = await _db.Attendances.FindAsync(attendanceId);
            if (attendance != null)
            {
                _db.Attendances.Remove(attendance);
                await _db.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Lesson>> GetLessonsWithoutAttendanceAsync()
        {
            var now = DateTime.Now;
            
            // Get completed or past lessons that should have attendance but don't
            var lessonsWithoutAttendance = await _db.Lessons
                .Include(l => l.Group)
                .Include(l => l.Attendances)
                .Include(l => l.Group)
                    .ThenInclude(g => g.Enrollments)
                .Where(l => (l.Status == LessonStatus.Completed || l.StartTime < now) &&
                           l.Group.Enrollments.Any())
                .ToListAsync();

            // Filter to only those missing attendance for at least one enrolled student
            var result = new List<Lesson>();
            foreach (var lesson in lessonsWithoutAttendance)
            {
                var enrolledStudentIds = lesson.Group.Enrollments.Select(e => e.StudentId).ToList();
                var attendanceStudentIds = lesson.Attendances.Select(a => a.StudentId).ToList();
                
                // If any enrolled student doesn't have attendance, this lesson needs attention
                if (enrolledStudentIds.Any(id => !attendanceStudentIds.Contains(id)))
                {
                    result.Add(lesson);
                }
            }

            return result.OrderByDescending(l => l.StartTime);
        }
    }
}
