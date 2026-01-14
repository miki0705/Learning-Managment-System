using Microsoft.EntityFrameworkCore;
using Learning_Management_System.Models;
using System;

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
        }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=LearningManagementSystem.db");
                optionsBuilder.UseLazyLoadingProxies();
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // USUNIĘTO: Konwerter TimeSpan na Ticks (powodował błędy przy edycji)
            // Teraz StartTime i EndTime są typem DateTime, który SQLite obsługuje natywnie.

            // Global Query Filters for Soft Delete
            modelBuilder.Entity<Student>().HasQueryFilter(s => !s.IsDeleted);
            modelBuilder.Entity<Group>().HasQueryFilter(g => !g.IsDeleted);
            modelBuilder.Entity<Lesson>().HasQueryFilter(l => !l.IsDeleted);
            modelBuilder.Entity<Payment>().HasQueryFilter(p => !p.IsDeleted);
            modelBuilder.Entity<Attendance>().HasQueryFilter(a => !a.IsDeleted);
            modelBuilder.Entity<Enrollment>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<GroupSchedule>().HasQueryFilter(gs => !gs.IsDeleted);

            modelBuilder.Entity<GroupSchedule>(entity =>
            {
                // Relacja z grupą
                entity.HasOne(e => e.Group)
                      .WithMany(g => g.Schedules)
                      .HasForeignKey(e => e.GroupId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // 1. Unikalność zapisu ucznia do grupy
            modelBuilder.Entity<Enrollment>()
                .HasIndex(e => new { e.StudentId, e.GroupId })
                .IsUnique();

            // 2. Relacja Lekcji z Grupą
            modelBuilder.Entity<Lesson>()
                .HasOne(l => l.Group)
                .WithMany(g => g.Lessons)
                .HasForeignKey(l => l.GroupId)
                .OnDelete(DeleteBehavior.Restrict); // Changed to Restrict to preserve historical data

            // 3. Relacja Lekcji z Harmonogramem
            modelBuilder.Entity<Lesson>()
                .HasOne(l => l.GroupSchedule)
                .WithMany()
                .HasForeignKey(l => l.GroupScheduleId)
                .OnDelete(DeleteBehavior.SetNull);

            // 4. Relacja Attendance z Lesson
            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.Lesson)
                .WithMany(l => l.Attendances)
                .HasForeignKey(a => a.LessonId)
                .OnDelete(DeleteBehavior.Restrict); // Changed to Restrict to preserve historical data

            // 5. Relacja Attendance z Student
            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.Student)
                .WithMany(s => s.Attendances)
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Restrict); // Changed to Restrict to preserve historical data

            // 6. Relacja Payment z Student
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Student)
                .WithMany(s => s.Payments)
                .HasForeignKey(p => p.StudentId)
                .OnDelete(DeleteBehavior.Restrict); // Changed to Restrict to preserve historical data
        }
    }
}