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
    public class StudentServiceTests
    {
        private DbContextOptions<SchoolDbContext> _options;
        private Mock<ILogger<StudentService>> _mockLogger;

        public StudentServiceTests()
        {
            _options = new DbContextOptionsBuilder<SchoolDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique DB per test
                .Options;
            
            _mockLogger = new Mock<ILogger<StudentService>>();
        }

        private SchoolDbContext CreateContext()
        {
            return new SchoolDbContext(_options);
        }

        [Fact]
        public async Task GetAllStudentsAsync_ReturnsStudents()
        {
            // Arrange
            using (var context = CreateContext())
            {
                context.Students.Add(new Student { FirstName = "John", LastName = "Doe", DateOfBirth = DateTime.Now });
                context.Students.Add(new Student { FirstName = "Jane", LastName = "Doe", DateOfBirth = DateTime.Now });
                await context.SaveChangesAsync();
            }

            using (var context = CreateContext())
            {
                var service = new StudentService(context, _mockLogger.Object);

                // Act
                var result = await service.GetAllStudentsAsync();

                // Assert
                Assert.NotNull(result);
                Assert.Equal(2, result.Count);
                Assert.Contains(result, s => s.FirstName == "John");
                Assert.Contains(result, s => s.FirstName == "Jane");
            }
        }

        [Fact]
        public async Task AddStudentAsync_AddsStudent()
        {
            // Arrange
            var studentModel = new StudentViewModel
            {
                FirstName = "New",
                LastName = "Student",
                DateOfBirth = DateTime.Now,
                Class = "Test Class"
            };

            using (var context = CreateContext())
            {
                var service = new StudentService(context, _mockLogger.Object);

                // Act
                await service.AddStudentAsync(studentModel, 1);
            }

            // Assert
            using (var context = CreateContext())
            {
                var count = await context.Students.CountAsync();
                Assert.Equal(1, count);
                var savedStudent = await context.Students.FirstOrDefaultAsync();
                Assert.Equal("New", savedStudent.FirstName);
                Assert.Equal(1, savedStudent.SchoolClassId);
            }
        }

        [Fact]
        public async Task AddStudentAsync_ViewModel_ThrowsOnInvalidId()
        {
            using (var context = CreateContext())
            {
                var service = new StudentService(context, _mockLogger.Object);
                var model = new StudentViewModel
                {
                    FirstName = "Bad",
                    LastName = "Class",
                    Class = "NotAnInt",
                    ClassId = null // Ensure null to test fallback or failure
                };

                // Act & Assert
                await Assert.ThrowsAsync<ArgumentException>(() => service.AddStudentAsync(model));
            }
        }

        [Fact]
        public async Task AddStudentAsync_ViewModel_UsesClassId()
        {
            using (var context = CreateContext())
            {
                var service = new StudentService(context, _mockLogger.Object);
                var model = new StudentViewModel
                {
                    FirstName = "Good",
                    LastName = "Class",
                    Class = "NotAnInt",
                    ClassId = 99
                };

                await service.AddStudentAsync(model);
            }

            using (var context = CreateContext())
            {
                var savedStudent = await context.Students.FirstOrDefaultAsync();
                Assert.Equal(99, savedStudent.SchoolClassId);
            }
        }
    }
}
