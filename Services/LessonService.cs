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

        public async Task SyncLessonsWithScheduleAsync(Group group)
        {
            if (group == null || group.Schedules == null) return;

            int weeksToGenerate = 10;
            DateTime startDate = DateTime.Today;

            foreach (var schedule in group.Schedules)
            {
                // 1. POBIERZ I USUŃ wszystkie przyszłe lekcje powiązane z tym grafikiem, 
                //    które nie zostały jeszcze "zrealizowane" (np. status Scheduled).
                //    To zapobiega duplikatom przy zmianie godziny/dnia.
                var futureLessons = await _context.Lessons
                    .Where(l => l.GroupScheduleId == schedule.Id &&
                                l.StartTime >= startDate &&
                                l.Status == LessonStatus.Scheduled)
                    .ToListAsync();

                if (futureLessons.Any())
                {
                    _context.Lessons.RemoveRange(futureLessons);
                    // Zapisujemy zmiany od razu, aby LessonExists nie widział usuwanych lekcji
                    await _context.SaveChangesAsync();
                }

                // 2. GENERUJ LEKCJE NA NOWO według zaktualizowanego grafiku
                for (int i = 0; i < weeksToGenerate; i++)
                {
                    DateTime lessonDate = startDate.AddDays(i * 7);
                    int daysUntilNextDay = ((int)schedule.DayOfWeek - (int)lessonDate.DayOfWeek + 7) % 7;
                    lessonDate = lessonDate.AddDays(daysUntilNextDay);

                    DateTime finalStart = lessonDate.Date.Add(schedule.StartTime.TimeOfDay);
                    DateTime finalEnd = lessonDate.Date.Add(schedule.EndTime.TimeOfDay);

                    // Sprawdzamy czy lekcja już istnieje (na wypadek gdyby 
                    // użytkownik dodał coś ręcznie o tej samej porze)
                    if (!await LessonExistsAsync(group.Id, schedule.Id, finalStart))
                    {
                        _context.Lessons.Add(new Lesson
                        {
                            GroupId = group.Id,
                            GroupScheduleId = schedule.Id,
                            StartTime = finalStart,
                            EndTime = finalEnd,
                            Status = LessonStatus.Scheduled,
                            Note = "Lekcja generowana automatycznie"
                        });
                    }
                }
            }
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Lesson>> GetFutureLessonsByScheduleAsync(int scheduleId)
        {
            return await _context.Lessons
                .Where(l => l.GroupScheduleId == scheduleId && l.StartTime > DateTime.Now)
                .ToListAsync();
        }

        public async Task<bool> LessonExistsAsync(int groupId, int scheduleId, DateTime startTime)
        {
            return await _context.Lessons.AnyAsync(l =>
                l.GroupId == groupId &&
                l.GroupScheduleId == scheduleId &&
                l.StartTime == startTime);
        }

        public async Task CompleteLessonReportAsync(int lessonId, IPaymentService paymentService)
        {
            var lesson = await _context.Lessons
                .Include(l => l.Group)
                .Include(l => l.Attendances)
                .FirstOrDefaultAsync(l => l.Id == lessonId);

            if (lesson == null) return;

            // Update lesson status to Completed
            lesson.Status = LessonStatus.Completed;
            _context.Entry(lesson).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            // Update wallets from lesson
            await paymentService.UpdateWalletsFromLessonAsync(lessonId);
        }
    }
}