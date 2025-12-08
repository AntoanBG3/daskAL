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
    public class GradeServiceTests
    {
        private DbContextOptions<SchoolDbContext> _options;
        private Mock<ILogger<GradeService>> _mockLogger;

        public GradeServiceTests()
        {
            _options = new DbContextOptionsBuilder<SchoolDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _mockLogger = new Mock<ILogger<GradeService>>();
        }

        private SchoolDbContext CreateContext()
        {
            return new SchoolDbContext(_options);
        }

        [Fact]
        public async Task AddGradeAsync_AddsGrade_WithSubjectName()
        {
            using (var context = CreateContext())
            {
                context.Subjects.Add(new Subject { Id = 10, Name = "Math" });
                await context.SaveChangesAsync();

                var service = new GradeService(context, _mockLogger.Object);
                var model = new GradeViewModel
                {
                    StudentId = 1,
                    SubjectId = 10,
                    Value = 5
                };

                await service.AddGradeAsync(model);
            }

            using (var context = CreateContext())
            {
                var grade = await context.Grades.FirstOrDefaultAsync();
                Assert.NotNull(grade);
                Assert.Equal("Math", grade.SubjectName);
                Assert.Equal(5, grade.Value);
            }
        }
    }
}
