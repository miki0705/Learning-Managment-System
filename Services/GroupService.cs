using Learning_Management_System.Data;
using Learning_Management_System.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

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
            _db.Groups.Remove(group);
        }

        public void SaveChanges()
        {
            _db.SaveChanges();
        }

        public void ReloadGroup(Group group)
        {
            var entry = _db.Entry(group);
            if (entry.State == EntityState.Detached) return;

            entry.Reload();

            // Detach brudnych zmian, aby uniknąć konfliktów przy Reload
            var changedEnrollments = _db.ChangeTracker.Entries<Enrollment>()
                .Where(e => e.Entity.GroupId == group.Id &&
                           (e.State == EntityState.Added || e.State == EntityState.Deleted || e.State == EntityState.Modified))
                .ToList();
            foreach (var e in changedEnrollments) e.State = EntityState.Detached;

            var changedSchedules = _db.ChangeTracker.Entries<GroupSchedule>()
                .Where(e => e.Entity.GroupId == group.Id &&
                           (e.State == EntityState.Added || e.State == EntityState.Deleted || e.State == EntityState.Modified))
                .ToList();
            foreach (var s in changedSchedules) s.State = EntityState.Detached;

            entry.Collection(g => g.Enrollments).Load();
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