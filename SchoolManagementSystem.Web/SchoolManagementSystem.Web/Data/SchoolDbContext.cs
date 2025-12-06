using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Web.Models;

namespace SchoolManagementSystem.Web.Data
{
    public class SchoolDbContext : DbContext
    {
        public SchoolDbContext(DbContextOptions<SchoolDbContext> options) : base(options)
        {
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Grade> Grades { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure relationships if necessary, though conventions will likely handle most.
            // Example:
            // modelBuilder.Entity<Subject>()
            //    .HasOne(s => s.Teacher)
            //    .WithMany(t => t.TeachingSubjects)
            //    .HasForeignKey(s => s.TeacherId);
        }
    }
}
