using Microsoft.EntityFrameworkCore;
using Learning_Management_System.Models;

namespace Learning_Management_System.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Student> Students { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupSchedule> GroupSchedules { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Payment> Payments { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=LearningManagementSystem.db");

            //zeby 'virtual' dzialalo i bylo szybsze
            optionsBuilder.UseLazyLoadingProxies();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Opcjonalnie: możemy tu dodać unikalność, np. żeby nie dało się 
            // zapisać dwa razy tego samego ucznia do tej samej grupy
            modelBuilder.Entity<Enrollment>()
                .HasIndex(e => new { e.StudentId, e.GroupId })
                .IsUnique();
        }

    }
}
