using Learning_Management_System.Models;
using System.Collections.Generic;

namespace Learning_Management_System.Services
{
    public interface IGroupService
    {
        IEnumerable<Group> GetAllGroups();
        void AddGroup(Group group);
        void DeleteGroup(Group group);
        void SaveChanges();
        void ReloadGroup(Group group);
        Group CreateNewGroup(); // Tworzenie obiektu zgodnie z Twoim modelem

        // Zarządzanie studentami w grupie
        void UpdateGroupMembers(Group group, List<Student> selectedStudents);

        bool IsNew(Group group);
    }
}