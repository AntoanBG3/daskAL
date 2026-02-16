using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SchoolManagementSystem.Web.Services;
using SchoolManagementSystem.Web.Data;
using SchoolManagementSystem.Web.Models;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System;

namespace SchoolManagementSystem.Tests
{
    public class DataImportServiceTests
    {
        private readonly DbContextOptions<SchoolDbContext> _options;
        private readonly Mock<ILogger<DataImportService>> _mockLogger;

        public DataImportServiceTests()
        {
            _options = new DbContextOptionsBuilder<SchoolDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _mockLogger = new Mock<ILogger<DataImportService>>();
        }

        private SchoolDbContext CreateContext() => new SchoolDbContext(_options);

        // ── Teacher Import ─────────────────────────────────────────────

        [Fact]
        public async Task ImportFromJsonAsync_ImportsTeachers()
        {
            var json = @"{
                ""Teachers"": [
                    { ""FirstName"": ""John"", ""LastName"": ""Doe"", ""TeachingSubjects"": [""Math""] }
                ]
            }";

            using (var context = CreateContext())
            {
                var service = new DataImportService(context, _mockLogger.Object);
                await service.ImportFromJsonAsync(json);
            }

            using (var context = CreateContext())
            {
                var teacher = await context.Teachers.FirstOrDefaultAsync();
                Assert.NotNull(teacher);
                Assert.Equal("John", teacher.FirstName);
                Assert.Equal("Doe", teacher.LastName);
            }
        }

        // ── Subject Import + Teacher Linking ───────────────────────────

        [Fact]
        public async Task ImportFromJsonAsync_ImportsSubjectsAndLinksTeacher()
        {
            var json = @"{
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
                await service.ImportFromJsonAsync(json);
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

        // ── Full End-to-End Import ─────────────────────────────────────

        [Fact]
        public async Task ImportFromJsonAsync_FullImport_AllEntitiesCreated()
        {
            var json = @"{
                ""Teachers"": [
                    { ""FirstName"": ""Jane"", ""LastName"": ""Smith"", ""TeachingSubjects"": [""Science""] }
                ],
                ""Subjects"": [
                    { ""Name"": ""Science"", ""Description"": ""Natural Science"" }
                ],
                ""Students"": [
                    {
                        ""FirstName"": ""Alice"",
                        ""LastName"": ""Wonder"",
                        ""Class"": ""10A"",
                        ""DateOfBirth"": ""2010-05-15"",
                        ""SubjectGrades"": { ""Science"": [5, 6] }
                    }
                ]
            }";

            using (var context = CreateContext())
            {
                var service = new DataImportService(context, _mockLogger.Object);
                await service.ImportFromJsonAsync(json);
            }

            using (var context = CreateContext())
            {
                // Teacher
                Assert.Equal(1, await context.Teachers.CountAsync());

                // Subject linked to teacher
                var subject = await context.Subjects.FirstAsync();
                Assert.Equal("Science", subject.Name);
                Assert.NotNull(subject.TeacherId);

                // Class
                var schoolClass = await context.SchoolClasses.FirstAsync();
                Assert.Equal("10A", schoolClass.Name);

                // Student linked to class
                var student = await context.Students.Include(s => s.Grades).FirstAsync();
                Assert.Equal("Alice", student.FirstName);
                Assert.Equal(schoolClass.Id, student.SchoolClassId);

                // Grades linked to subject
                Assert.Equal(2, student.Grades.Count);
                Assert.All(student.Grades, g =>
                {
                    Assert.Equal("Science", g.SubjectName);
                    Assert.Equal(subject.Id, g.SubjectId);
                });
            }
        }

        // ── Invalid JSON ───────────────────────────────────────────────

        [Fact]
        public async Task ImportFromJsonAsync_InvalidJson_Throws()
        {
            var badJson = "{ this is not valid json }";

            using var context = CreateContext();
            var service = new DataImportService(context, _mockLogger.Object);

            await Assert.ThrowsAsync<System.Text.Json.JsonException>(
                () => service.ImportFromJsonAsync(badJson));
        }

        // ── Null Deserialization ────────────────────────────────────────

        [Fact]
        public async Task ImportFromJsonAsync_NullData_DoesNotThrow()
        {
            var nullJson = "null";

            using var context = CreateContext();
            var service = new DataImportService(context, _mockLogger.Object);

            // Should log a warning and return gracefully
            await service.ImportFromJsonAsync(nullJson);

            Assert.Equal(0, await context.Teachers.CountAsync());
        }
    }
}
