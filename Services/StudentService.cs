using Learning_Management_System.Data;
using Learning_Management_System.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Learning_Management_System.Services
{
    public class StudentService : IStudentService
    {
        private readonly AppDbContext _db;
        public StudentService(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Not really all, just all active. Needs change in future.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Student> GetAllStudents()
        {
            // Pobieramy tylko tych, którzy nie są "usunięci"
            return _db.Students
                       .Where(s => s.IsActive)
                       .OrderBy(s => s.LastName)
                       .ToList();
        }

        public void AddStudent(Student student)
        {
            _db.Students.Add(student);
        }

        public void DeleteStudent(Student student)
        {
            student.IsDeleted = true;
            student.IsActive = false;
        }
        public void SaveChanges()
        {
            _db.SaveChanges();
        }

        public void ReloadStudent(Student student)
        {
            if (student.Id == 0) return;

            var entry = _db.Entry(student);
            if ((entry.State != EntityState.Detached))
            {
                entry.Reload();
            }
        }

        public Student CreateNewStudent()
        {
            return new Student { FirstName = "Nowy", LastName = "Uczeń" };
        }
        public bool IsNew(Student student)
        {
            return _db.Entry(student).State == EntityState.Added || student.Id == 0;
        }
    }
}
