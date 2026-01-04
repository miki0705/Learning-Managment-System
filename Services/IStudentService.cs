using Learning_Management_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning_Management_System.Services
{
    public interface IStudentService
    {
        IEnumerable<Student> GetAllStudents();
        void AddStudent(Student student);
        void DeleteStudent(Student student);
        void SaveChanges();
        void ReloadStudent(Student student);
        Student CreateNewStudent();
        bool IsNew(Student student);
    }
}
