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
    public class TeacherServiceTests
    {
        private DbContextOptions<SchoolDbContext> _options;
        private Mock<ILogger<TeacherService>> _mockLogger;

        public TeacherServiceTests()
        {
            _options = new DbContextOptionsBuilder<SchoolDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _mockLogger = new Mock<ILogger<TeacherService>>();
        }

        private SchoolDbContext CreateContext()
        {
            return new SchoolDbContext(_options);
        }

        [Fact]
        public async Task AddTeacherAsync_AddsTeacher_WithUserId()
        {
            using (var context = CreateContext())
            {
                var service = new TeacherService(context, _mockLogger.Object);
                var model = new TeacherViewModel { FirstName = "T", LastName = "User" };

                await service.AddTeacherAsync(model, "user-123");
            }

            using (var context = CreateContext())
            {
                var teacher = await context.Teachers.FirstOrDefaultAsync();
                Assert.NotNull(teacher);
                Assert.Equal("user-123", teacher.UserId);
            }
        }

        [Fact]
        public async Task GetTeacherByIdAsync_ReturnsTeacher()
        {
            using (var context = CreateContext())
            {
                context.Teachers.Add(new Teacher { Id = 1, FirstName = "T", LastName = "One" });
                await context.SaveChangesAsync();
            }

            using (var context = CreateContext())
            {
                var service = new TeacherService(context, _mockLogger.Object);
                var result = await service.GetTeacherByIdAsync(1);

                Assert.NotNull(result);
                Assert.Equal("T", result.FirstName);
            }
        }

        [Fact]
        public async Task UpdateTeacherAsync_UpdatesTeacher()
        {
            using (var context = CreateContext())
            {
                context.Teachers.Add(new Teacher { Id = 2, FirstName = "Old", LastName = "Name" });
                await context.SaveChangesAsync();
            }

            using (var context = CreateContext())
            {
                var service = new TeacherService(context, _mockLogger.Object);
                var model = new TeacherViewModel { Id = 2, FirstName = "New", LastName = "Name" };
                await service.UpdateTeacherAsync(model);
            }

            using (var context = CreateContext())
            {
                var teacher = await context.Teachers.FindAsync(2);
                Assert.Equal("New", teacher.FirstName);
            }
        }

        [Fact]
        public async Task DeleteTeacherAsync_DeletesTeacher()
        {
            using (var context = CreateContext())
            {
                context.Teachers.Add(new Teacher { Id = 3, FirstName = "To", LastName = "Delete" });
                await context.SaveChangesAsync();
            }

            using (var context = CreateContext())
            {
                var service = new TeacherService(context, _mockLogger.Object);
                await service.DeleteTeacherAsync(3);
            }

            using (var context = CreateContext())
            {
                Assert.Empty(context.Teachers);
            }
        }
    }
}
