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
    public class AbsenceServiceTests
    {
        private DbContextOptions<SchoolDbContext> _options;
        private Mock<ILogger<AbsenceService>> _mockLogger;

        public AbsenceServiceTests()
        {
            _options = new DbContextOptionsBuilder<SchoolDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _mockLogger = new Mock<ILogger<AbsenceService>>();
        }

        private SchoolDbContext CreateContext()
        {
            return new SchoolDbContext(_options);
        }

        [Fact]
        public async Task AddAbsenceAsync_AddsAbsence()
        {
            using (var context = CreateContext())
            {
                var service = new AbsenceService(context, _mockLogger.Object);
                await service.AddAbsenceAsync(1, 10, true);
            }

            using (var context = CreateContext())
            {
                var abs = await context.Absences.FirstOrDefaultAsync();
                Assert.NotNull(abs);
                Assert.Equal(1, abs.StudentId);
                Assert.Equal(10, abs.SubjectId);
                Assert.True(abs.IsExcused);
                Assert.Equal(DateTime.Now.Date, abs.Date.Date);
            }
        }
    }
}
