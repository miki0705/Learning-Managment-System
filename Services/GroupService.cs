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
            // Include(g => g.Enrollments) jest ważne, żeby EF widział powiązania
            return _db.Groups.Include(g => g.Enrollments).OrderBy(g => g.Name).ToList();
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
            if (entry.State != EntityState.Detached) 
                entry.Reload();
        }

        public Group CreateNewGroup()
        {
            return new Group
            {
                Name = "Nowa Grupa",
                Level = "A1",
                BaseRate = 0
            };
        }

        public void UpdateGroupMembers(Group group, List<Student> selectedStudents)
        {
            // 1. Znajdujemy i usuwamy stare wpisy dla tej grupy
            var existingEnrollments = _db.Enrollments.Where(e => e.GroupId == group.Id);
            _db.Enrollments.RemoveRange(existingEnrollments);

            // 2. Dodajemy nowe wpisy dla każdego zaznaczonego studenta
            foreach (var student in selectedStudents)
            {
                _db.Enrollments.Add(new Enrollment
                {
                    GroupId = group.Id,
                    StudentId = student.Id,
                });
            }
        }
        public bool IsNew(Group group)
        {
            return _db.Entry(group).State == EntityState.Added || group.Id == 0;
        }
    }
}