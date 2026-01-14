using Learning_Management_System.Data;
using Learning_Management_System.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Learning_Management_System.Services
{
    public class GroupService : IGroupService
    {
        private readonly AppDbContext _db;

        public GroupService(AppDbContext db)
        {
            _db = db;
        }

        public IEnumerable<Group> GetAllGroups()
        {
            return _db.Groups
                .Include(g => g.Enrollments)
                    .ThenInclude(e => e.Student)
                .Include(g => g.Schedules)
                .OrderBy(g => g.Name)
                .ToList();
        }

        public void AddGroup(Group group)
        {
            _db.Groups.Add(group);
        }

        public void DeleteGroup(Group group)
        {
            // Ensure related entities are loaded
            _db.Entry(group).Collection(g => g.Enrollments).Load();
            _db.Entry(group).Collection(g => g.Schedules).Load();
            
            group.IsDeleted = true;
            _db.Entry(group).State = EntityState.Modified;
            
            // Soft delete related Enrollments
            foreach (var enrollment in group.Enrollments)
            {
                enrollment.IsDeleted = true;
                _db.Entry(enrollment).State = EntityState.Modified;
            }
            
            // Soft delete related GroupSchedules
            foreach (var schedule in group.Schedules)
            {
                schedule.IsDeleted = true;
                _db.Entry(schedule).State = EntityState.Modified;
            }
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }

        public void ReloadGroup(Group group)
        {
            var entry = _db.Entry(group);
            if (entry.State == EntityState.Detached) return;

            entry.Reload();

            // Detach ALL Enrollment entities for this group (not just modified ones) to ensure clean reload
            var allEnrollments = _db.ChangeTracker.Entries<Enrollment>()
                .Where(e => e.Entity.GroupId == group.Id)
                .ToList();
            foreach (var e in allEnrollments) e.State = EntityState.Detached;

            var changedSchedules = _db.ChangeTracker.Entries<GroupSchedule>()
                .Where(e => e.Entity.GroupId == group.Id &&
                           (e.State == EntityState.Added || e.State == EntityState.Deleted || e.State == EntityState.Modified))
                .ToList();
            foreach (var s in changedSchedules) s.State = EntityState.Detached;

            // Clear the Enrollments collection first to ensure clean reload
            if (group.Enrollments != null)
            {
                group.Enrollments.Clear();
            }

            // Reload the Enrollments collection with fresh data from database
            _db.Entry(group).Collection(g => g.Enrollments).Query().Include(e => e.Student).Load();

            entry.Collection(g => g.Schedules).Load();
        }

        public Group CreateNewGroup()
        {
            return new Group
            {
                Name = "Nowa Grupa",
                Level = "A1",
                BaseRate = 0,
                Schedules = new List<GroupSchedule>(),
                Enrollments = new List<Enrollment>()
            };
        }

        public void UpdateGroupMembers(Group group, List<Student> selectedStudents)
        {
            // WAŻNE: Pracujemy na ID, aby uniknąć problemów z instancjami obiektów
            var targetStudentIds = selectedStudents.Select(s => s.Id).ToList();

            // 1. Usuwamy te osoby, których nie ma w nowym wyborze
            var toRemove = group.Enrollments
                .Where(e => !targetStudentIds.Contains(e.StudentId))
                .ToList();

            foreach (var enrollment in toRemove)
            {
                group.Enrollments.Remove(enrollment);
                // Jeśli rekord istnieje w bazie (Id > 0), oznaczamy do usunięcia w DB
                if (enrollment.GroupId != 0 && enrollment.StudentId != 0)
                {
                    _db.Enrollments.Remove(enrollment);
                }
            }

            // 2. Dodajemy tylko te osoby, których jeszcze nie ma w kolekcji
            var currentStudentIds = group.Enrollments.Select(e => e.StudentId).ToList();
            foreach (var studentId in targetStudentIds)
            {
                if (!currentStudentIds.Contains(studentId))
                {
                    group.Enrollments.Add(new Enrollment
                    {
                        GroupId = group.Id,
                        StudentId = studentId
                    });
                }
            }
        }

        public bool IsNew(Group group)
        {
            return _db.Entry(group).State == EntityState.Added || group.Id == 0;
        }
    }
}