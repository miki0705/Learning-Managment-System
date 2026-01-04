using Learning_Management_System.Data;
using Learning_Management_System.Models;
using System;
using Learning_Management_System.Data;
using Learning_Management_System.Models;
using Microsoft.EntityFrameworkCore;
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

        //to w sumie jeszcze nwm jak zrobic do konca, 
        //czy usuwac na stałe czy tak wyłączac, chyba nie ma to wiekszej roznicy jesli 
        //is active bedzie go wywalać z UI to user bedzie mogl dodac nowego
        public void DeleteStudent(Student student)
        {
            student.IsActive = false;
            //_db.Students.Remove(student);
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
