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
    public class ClassServiceTests
    {
        private DbContextOptions<SchoolDbContext> _options;
        private Mock<ILogger<ClassService>> _mockLogger;

        public ClassServiceTests()
        {
            _options = new DbContextOptionsBuilder<SchoolDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _mockLogger = new Mock<ILogger<ClassService>>();
        }

        private SchoolDbContext CreateContext()
        {
            return new SchoolDbContext(_options);
        }

        [Fact]
        public async Task AddClassAsync_AddsClass()
        {
            using (var context = CreateContext())
            {
                var service = new ClassService(context, _mockLogger.Object);
                await service.AddClassAsync(new SchoolClassViewModel { Name = "10A" });
            }

            using (var context = CreateContext())
            {
                Assert.Equal(1, await context.SchoolClasses.CountAsync());
                Assert.Equal("10A", (await context.SchoolClasses.FirstAsync()).Name);
            }
        }

        [Fact]
        public async Task UpdateClassSubjectsAsync_UpdatesSubjects()
        {
            using (var context = CreateContext())
            {
                context.SchoolClasses.Add(new SchoolClass { Id = 1, Name = "10A" });
                context.Subjects.Add(new Subject { Id = 101, Name = "Math" });
                context.Subjects.Add(new Subject { Id = 102, Name = "Physics" });
                await context.SaveChangesAsync();

                var service = new ClassService(context, _mockLogger.Object);
                await service.UpdateClassSubjectsAsync(1, new List<int> { 101, 102 });
            }

            using (var context = CreateContext())
            {
                var classSubjects = await context.ClassSubjects.Where(cs => cs.SchoolClassId == 1).ToListAsync();
                Assert.Equal(2, classSubjects.Count);
                Assert.Contains(classSubjects, cs => cs.SubjectId == 101);
                Assert.Contains(classSubjects, cs => cs.SubjectId == 102);
            }
        }
    }
}
