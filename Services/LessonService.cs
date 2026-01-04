using Microsoft.EntityFrameworkCore;
using Learning_Management_System.Data;
using Learning_Management_System.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Learning_Management_System.Services
{
    public class LessonService : ILessonService
    {
        private readonly AppDbContext _context;

        public LessonService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Lesson>> GetLessonsForDateRangeAsync(DateTime start, DateTime end)
        {
            return await _context.Lessons
                .Include(l => l.Group)
                .Where(l => l.StartTime >= start && l.StartTime <= end)
                .OrderBy(l => l.StartTime)
                .ToListAsync();
        }

        public async Task AddLessonAsync(Lesson lesson)
        {
            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateLessonAsync(Lesson lesson)
        {
            // Sprawdzamy, czy obiekt jest już śledzony przez EF
            var trackedEntity = _context.Lessons.Local.FirstOrDefault(l => l.Id == lesson.Id);
            if (trackedEntity != null)
            {
                _context.Entry(trackedEntity).State = EntityState.Detached;
            }

            _context.Entry(lesson).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteLessonAsync(int lessonId)
        {
            var lesson = await _context.Lessons.FindAsync(lessonId);
            if (lesson != null)
            {
                _context.Lessons.Remove(lesson);
                await _context.SaveChangesAsync();
            }
        }

        // Pobiera lekcje, które mają się odbyć w przyszłości dla konkretnego wpisu w grafiku
        public IEnumerable<Lesson> GetFutureLessonsBySchedule(int scheduleId)
        {
            return _context.Lessons
                .Where(l => l.GroupScheduleId == scheduleId && l.StartTime > DateTime.Now)
                .ToList();
        }

        // Sprawdza, czy lekcja o takich parametrach już istnieje, by uniknąć duplikatów
        public bool LessonExists(int groupId, int scheduleId, DateTime startTime)
        {
            return _context.Lessons.Any(l =>
                l.GroupId == groupId &&
                l.GroupScheduleId == scheduleId &&
                l.StartTime == startTime);
        }
    }
}