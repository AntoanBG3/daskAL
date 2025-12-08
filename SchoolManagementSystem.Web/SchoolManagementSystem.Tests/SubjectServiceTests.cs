using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SchoolManagementSystem.Web.Services;
using SchoolManagementSystem.Web.Data;
using SchoolManagementSystem.Web.Models.ViewModels;
using System.Threading.Tasks;
using System.Linq;
using SchoolManagementSystem.Web.Models;
using System.Collections.Generic;
using System;

namespace SchoolManagementSystem.Tests
{
    public class SubjectServiceTests
    {
        private DbContextOptions<SchoolDbContext> _options;
        private Mock<ILogger<SubjectService>> _mockLogger;

        public SubjectServiceTests()
        {
            _options = new DbContextOptionsBuilder<SchoolDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _mockLogger = new Mock<ILogger<SubjectService>>();
        }

        private SchoolDbContext CreateContext()
        {
            return new SchoolDbContext(_options);
        }

        [Fact]
        public async Task AddSubjectAsync_AddsSubject()
        {
            using (var context = CreateContext())
            {
                var service = new SubjectService(context, _mockLogger.Object);
                await service.AddSubjectAsync(new SubjectViewModel { Name = "Biology", Description = "Bio 101" });
            }

            using (var context = CreateContext())
            {
                var subject = await context.Subjects.FirstOrDefaultAsync();
                Assert.NotNull(subject);
                Assert.Equal("Biology", subject.Name);
            }
        }

        [Fact]
        public async Task GetAvailableSubjectsForStudentAsync_ReturnsLinkedSubjects()
        {
            using (var context = CreateContext())
            {
                var cls = new SchoolClass { Id = 1, Name = "10A" };
                var sub1 = new Subject { Id = 10, Name = "Math" };
                var sub2 = new Subject { Id = 11, Name = "English" };

                context.SchoolClasses.Add(cls);
                context.Subjects.AddRange(sub1, sub2);
                context.ClassSubjects.Add(new ClassSubject { SchoolClassId = 1, SubjectId = 10 }); // Only Math linked
                context.Students.Add(new Student { Id = 100, FirstName = "S", LastName = "T", SchoolClassId = 1 });

                await context.SaveChangesAsync();

                var service = new SubjectService(context, _mockLogger.Object);
                var result = await service.GetAvailableSubjectsForStudentAsync(100);

                Assert.Single(result);
                Assert.Equal("Math", result.First().Name);
            }
        }
    }
}
