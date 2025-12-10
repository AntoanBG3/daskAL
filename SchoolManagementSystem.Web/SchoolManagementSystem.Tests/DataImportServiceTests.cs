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
    public class DataImportServiceTests
    {
        private DbContextOptions<SchoolDbContext> _options;
        private Mock<ILogger<DataImportService>> _mockLogger;

        public DataImportServiceTests()
        {
            _options = new DbContextOptionsBuilder<SchoolDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _mockLogger = new Mock<ILogger<DataImportService>>();
        }

        private SchoolDbContext CreateContext()
        {
            return new SchoolDbContext(_options);
        }

        [Fact]
        public async Task ImportFromJsonAsync_ImportsTeachers()
        {
            // Arrange
            var json = @"{
                ""Teachers"": [
                    { ""FirstName"": ""John"", ""LastName"": ""Doe"", ""TeachingSubjects"": [""Math""] }
                ]
            }";

            using (var context = CreateContext())
            {
                var service = new DataImportService(context, _mockLogger.Object);

                // Act
                await service.ImportFromJsonAsync(json);
            }

            // Assert
            using (var context = CreateContext())
            {
                var teacher = await context.Teachers.FirstOrDefaultAsync();
                Assert.NotNull(teacher);
                Assert.Equal("John", teacher.FirstName);
                Assert.Equal("Doe", teacher.LastName);
            }
        }

        [Fact]
        public async Task ImportFromJsonAsync_ImportsSubjectsAndLinksTeacher()
        {
            // Arrange
            var jsonCombined = @"{
                ""Teachers"": [
                    { ""FirstName"": ""John"", ""LastName"": ""Doe"", ""TeachingSubjects"": [""Math""] }
                ],
                ""Subjects"": [
                    { ""Name"": ""Math"", ""Description"": ""Mathematics"" }
                ]
            }";

            using (var context = CreateContext())
            {
                var service = new DataImportService(context, _mockLogger.Object);
                await service.ImportFromJsonAsync(jsonCombined);
            }

            using (var context = CreateContext())
            {
                var teacher = await context.Teachers.FirstOrDefaultAsync();
                var subject = await context.Subjects.FirstOrDefaultAsync();

                Assert.NotNull(teacher);
                Assert.NotNull(subject);
                Assert.Equal("Math", subject.Name);
                Assert.Equal(teacher.Id, subject.TeacherId);
            }
        }
    }
}
