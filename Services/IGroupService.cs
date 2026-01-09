using Learning_Management_System.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Learning_Management_System.Services
{
    public interface IGroupService
    {
        IEnumerable<Group> GetAllGroups();
        void AddGroup(Group group);
        void DeleteGroup(Group group);
        Task SaveChangesAsync();
        void ReloadGroup(Group group);
        Group CreateNewGroup(); // Tworzenie obiektu zgodnie z Twoim modelem

        // Zarządzanie studentami w grupie
        void UpdateGroupMembers(Group group, List<Student> selectedStudents);

        bool IsNew(Group group);
    }
}