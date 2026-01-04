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

        public AppDbContext()
        {
            // PROFESJONALNE PODEJŚCIE:
            // Tworzy bazę danych TYLKO, jeśli plik jeszcze nie istnieje.
            // Jeśli plik już jest, EF po prostu go użyje, zachowując dane.
            Database.EnsureCreated();
        }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Plik będzie się znajdował w folderze z plikiem .exe (bin/Debug)
                optionsBuilder.UseSqlite("Data Source=LearningManagementSystem.db");
                optionsBuilder.UseLazyLoadingProxies();
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 1. Unikalność zapisu ucznia do grupy
            modelBuilder.Entity<Enrollment>()
                .HasIndex(e => new { e.StudentId, e.GroupId })
                .IsUnique();

            // 2. Relacja Lekcji z Grupą
            modelBuilder.Entity<Lesson>()
                .HasOne(l => l.Group)
                .WithMany()
                .HasForeignKey(l => l.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            // 3. Relacja Lekcji z Harmonogramem (Opcja B)
            modelBuilder.Entity<Lesson>()
                .HasOne(l => l.GroupSchedule)
                .WithMany()
                .HasForeignKey(l => l.GroupScheduleId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}