using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SchoolManagementSystem.Web.Data;
using SchoolManagementSystem.Web.Models;
using SchoolManagementSystem.Web.Services;
using Xunit;

namespace SchoolManagementSystem.Tests.Services
{
    public class ScheduleServiceTests
    {
        private readonly SchoolDbContext _context;
        private readonly Mock<ILogger<ScheduleService>> _loggerMock;
        private readonly ScheduleService _service;

        public ScheduleServiceTests()
        {
            var options = new DbContextOptionsBuilder<SchoolDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new SchoolDbContext(options);
            _loggerMock = new Mock<ILogger<ScheduleService>>();
            _service = new ScheduleService(_context, _loggerMock.Object);
        }

        [Fact]
        public async Task AddScheduleEntryAsync_ShouldAddEntry_WhenNoConflict()
        {
            // Arrange
            var teacher = new Teacher { Id = 1, FirstName = "John", LastName = "Doe" };
            var subject = new Subject { Id = 1, Name = "Math", TeacherId = teacher.Id };
            var schoolClass = new SchoolClass { Id = 1, Name = "Class A" };
            var classSubject = new ClassSubject { SchoolClassId = schoolClass.Id, SubjectId = subject.Id, Subject = subject, SchoolClass = schoolClass };

            _context.Teachers.Add(teacher);
            _context.Subjects.Add(subject);
            _context.SchoolClasses.Add(schoolClass);
            _context.ClassSubjects.Add(classSubject);
            await _context.SaveChangesAsync();

            // Act
            await _service.AddScheduleEntryAsync(1, 1, DayOfWeek.Monday, new TimeSpan(8, 0, 0), new TimeSpan(9, 0, 0), "101");

            // Assert
            var entry = await _context.ScheduleEntries.FirstOrDefaultAsync();
            Assert.NotNull(entry);
            Assert.Equal(1, entry.SchoolClassId);
            Assert.Equal(1, entry.SubjectId);
            Assert.Equal(DayOfWeek.Monday, entry.DayOfWeek);
            Assert.Equal(new TimeSpan(8, 0, 0), entry.StartTime);
            Assert.Equal(new TimeSpan(9, 0, 0), entry.EndTime);
            Assert.Equal("101", entry.RoomNumber);
        }

        [Fact]
        public async Task AddScheduleEntryAsync_ShouldThrow_WhenTeacherConflict()
        {
            // Arrange
            var teacher = new Teacher { Id = 1, FirstName = "John", LastName = "Doe" };
            var subject1 = new Subject { Id = 1, Name = "Math", TeacherId = teacher.Id }; // Teacher 1
            var subject2 = new Subject { Id = 2, Name = "Physics", TeacherId = teacher.Id }; // Teacher 1
            var schoolClass1 = new SchoolClass { Id = 1, Name = "Class A" };
            var schoolClass2 = new SchoolClass { Id = 2, Name = "Class B" };

            var cs1 = new ClassSubject { SchoolClassId = 1, SubjectId = 1, Subject = subject1, SchoolClass = schoolClass1 };
            var cs2 = new ClassSubject { SchoolClassId = 2, SubjectId = 2, Subject = subject2, SchoolClass = schoolClass2 };

            _context.Teachers.Add(teacher);
            _context.Subjects.AddRange(subject1, subject2);
            _context.SchoolClasses.AddRange(schoolClass1, schoolClass2);
            _context.ClassSubjects.AddRange(cs1, cs2);

            // Existing schedule for Class A, Subject 1 (Teacher 1) at Mon 8-9
            _context.ScheduleEntries.Add(new ScheduleEntry
            {
                SchoolClassId = 1,
                SubjectId = 1,
                DayOfWeek = DayOfWeek.Monday,
                StartTime = new TimeSpan(8, 0, 0),
                EndTime = new TimeSpan(9, 0, 0),
                ClassSubject = cs1
            });
            await _context.SaveChangesAsync();

            // Act & Assert
            // Try to schedule Class B, Subject 2 (Teacher 1) at Mon 8:30-9:30 (Overlap)
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.AddScheduleEntryAsync(2, 2, DayOfWeek.Monday, new TimeSpan(8, 30, 0), new TimeSpan(9, 30, 0), "102"));
        }

        [Fact]
        public async Task AddScheduleEntryAsync_ShouldThrow_WhenClassConflict()
        {
            // Arrange
            var teacher1 = new Teacher { Id = 1, FirstName = "John", LastName = "Doe" };
            var teacher2 = new Teacher { Id = 2, FirstName = "Jane", LastName = "Smith" };
            var subject1 = new Subject { Id = 1, Name = "Math", TeacherId = teacher1.Id };
            var subject2 = new Subject { Id = 2, Name = "English", TeacherId = teacher2.Id };
            var schoolClass = new SchoolClass { Id = 1, Name = "Class A" };

            var cs1 = new ClassSubject { SchoolClassId = 1, SubjectId = 1, Subject = subject1, SchoolClass = schoolClass };
            var cs2 = new ClassSubject { SchoolClassId = 1, SubjectId = 2, Subject = subject2, SchoolClass = schoolClass };

            _context.Teachers.AddRange(teacher1, teacher2);
            _context.Subjects.AddRange(subject1, subject2);
            _context.SchoolClasses.Add(schoolClass);
            _context.ClassSubjects.AddRange(cs1, cs2);

            // Existing schedule for Class A, Subject 1 at Mon 8-9
            _context.ScheduleEntries.Add(new ScheduleEntry
            {
                SchoolClassId = 1,
                SubjectId = 1,
                DayOfWeek = DayOfWeek.Monday,
                StartTime = new TimeSpan(8, 0, 0),
                EndTime = new TimeSpan(9, 0, 0),
                ClassSubject = cs1
            });
            await _context.SaveChangesAsync();

            // Act & Assert
            // Try to schedule Class A, Subject 2 at Mon 8:30-9:30 (Overlap)
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.AddScheduleEntryAsync(1, 2, DayOfWeek.Monday, new TimeSpan(8, 30, 0), new TimeSpan(9, 30, 0), "102"));
        }

        [Fact]
        public async Task AddScheduleEntryAsync_ShouldThrow_WhenRoomConflict()
        {
             // Arrange
            var teacher1 = new Teacher { Id = 1, FirstName = "John", LastName = "Doe" };
            var teacher2 = new Teacher { Id = 2, FirstName = "Jane", LastName = "Smith" };
            var subject1 = new Subject { Id = 1, Name = "Math", TeacherId = teacher1.Id };
            var subject2 = new Subject { Id = 2, Name = "English", TeacherId = teacher2.Id };
            var schoolClass1 = new SchoolClass { Id = 1, Name = "Class A" };
            var schoolClass2 = new SchoolClass { Id = 2, Name = "Class B" };

            var cs1 = new ClassSubject { SchoolClassId = 1, SubjectId = 1, Subject = subject1, SchoolClass = schoolClass1 };
            var cs2 = new ClassSubject { SchoolClassId = 2, SubjectId = 2, Subject = subject2, SchoolClass = schoolClass2 };

            _context.Teachers.AddRange(teacher1, teacher2);
            _context.Subjects.AddRange(subject1, subject2);
            _context.SchoolClasses.AddRange(schoolClass1, schoolClass2);
            _context.ClassSubjects.AddRange(cs1, cs2);

            // Existing schedule for Class A, Room 101 at Mon 8-9
            _context.ScheduleEntries.Add(new ScheduleEntry
            {
                SchoolClassId = 1,
                SubjectId = 1,
                DayOfWeek = DayOfWeek.Monday,
                StartTime = new TimeSpan(8, 0, 0),
                EndTime = new TimeSpan(9, 0, 0),
                RoomNumber = "101",
                ClassSubject = cs1
            });
            await _context.SaveChangesAsync();

            // Act & Assert
            // Try to schedule Class B, Room 101 at Mon 8:30-9:30 (Overlap)
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.AddScheduleEntryAsync(2, 2, DayOfWeek.Monday, new TimeSpan(8, 30, 0), new TimeSpan(9, 30, 0), "101"));
        }
    }
}
