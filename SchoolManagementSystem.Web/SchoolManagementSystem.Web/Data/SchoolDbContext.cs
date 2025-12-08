using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Web.Models;
using SchoolManagementSystem.Web.Models.Auth;

namespace SchoolManagementSystem.Web.Data
{
    public class SchoolDbContext : IdentityDbContext<User, Role, string>
    {
        public DbSet<SchoolClass> SchoolClasses { get; set; }
        public DbSet<ClassSubject> ClassSubjects { get; set; }

        public DbSet<Student> Students { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<Absence> Absences { get; set; }
        
        // Auth Tables
        public DbSet<LoginAttempt> LoginAttempts { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Message> Messages { get; set; }

        public SchoolDbContext(DbContextOptions<SchoolDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ClassSubject>()
                .HasKey(cs => new { cs.SchoolClassId, cs.SubjectId });

            modelBuilder.Entity<ClassSubject>()
                .HasOne(cs => cs.SchoolClass)
                .WithMany(c => c.ClassSubjects)
                .HasForeignKey(cs => cs.SchoolClassId);

            modelBuilder.Entity<ClassSubject>()
                .HasOne(cs => cs.Subject)
                .WithMany(s => s.ClassSubjects)
                .HasForeignKey(cs => cs.SubjectId);

            // Message Configuration
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany()
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
