using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SchoolManagementSystem.Web.Data;
using SchoolManagementSystem.Web.Models;

namespace SchoolManagementSystem.Web.Services
{
    public interface IScheduleService
    {
        Task AddScheduleEntryAsync(int schoolClassId, int subjectId, DayOfWeek dayOfWeek, TimeSpan startTime, TimeSpan endTime, string? roomNumber);
        Task<List<ScheduleEntry>> GetScheduleForClassAsync(int schoolClassId);
        Task<List<ScheduleEntry>> GetScheduleForTeacherAsync(int teacherId);
    }

    public class ScheduleService : BaseService<ScheduleService>, IScheduleService
    {
        private readonly SchoolDbContext _context;

        public ScheduleService(SchoolDbContext context, ILogger<ScheduleService> logger) : base(logger)
        {
            _context = context;
        }

        public async Task AddScheduleEntryAsync(int schoolClassId, int subjectId, DayOfWeek dayOfWeek, TimeSpan startTime, TimeSpan endTime, string? roomNumber)
        {
            await ExecuteSafeAsync(async () =>
            {
                // Basic Validation
                if (startTime >= endTime)
                {
                    throw new ArgumentException("Start time must be before end time.");
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // Verify ClassSubject exists
                    var classSubject = await _context.ClassSubjects
                        .Include(cs => cs.Subject)
                        .FirstOrDefaultAsync(cs => cs.SchoolClassId == schoolClassId && cs.SubjectId == subjectId);

                    if (classSubject == null)
                    {
                        throw new ArgumentException("The specified class-subject combination does not exist.");
                    }

                    var teacherId = classSubject.Subject?.TeacherId;

                    // 1. Check Teacher Availability (if subject has a teacher)
                    if (teacherId.HasValue)
                    {
                        var teacherConflict = await _context.ScheduleEntries
                            .Include(se => se.ClassSubject)
                            .ThenInclude(cs => cs.Subject)
                            .AnyAsync(se =>
                                se.ClassSubject.Subject.TeacherId == teacherId.Value &&
                                se.DayOfWeek == dayOfWeek &&
                                se.StartTime < endTime && se.EndTime > startTime);

                        if (teacherConflict)
                        {
                            throw new InvalidOperationException("The teacher is already booked during this time slot.");
                        }
                    }

                    // 2. Check Class Availability
                    var classConflict = await _context.ScheduleEntries
                        .AnyAsync(se =>
                            se.SchoolClassId == schoolClassId &&
                            se.DayOfWeek == dayOfWeek &&
                            se.StartTime < endTime && se.EndTime > startTime);

                    if (classConflict)
                    {
                        throw new InvalidOperationException("The class is already booked during this time slot.");
                    }

                    // 3. Check Room Availability (if room is specified)
                    if (!string.IsNullOrEmpty(roomNumber))
                    {
                        var roomConflict = await _context.ScheduleEntries
                            .AnyAsync(se =>
                                se.RoomNumber == roomNumber &&
                                se.DayOfWeek == dayOfWeek &&
                                se.StartTime < endTime && se.EndTime > startTime);

                        if (roomConflict)
                        {
                            throw new InvalidOperationException($"Room {roomNumber} is already booked during this time slot.");
                        }
                    }

                    var entry = new ScheduleEntry
                    {
                        SchoolClassId = schoolClassId,
                        SubjectId = subjectId,
                        DayOfWeek = dayOfWeek,
                        StartTime = startTime,
                        EndTime = endTime,
                        RoomNumber = roomNumber
                    };

                    _context.ScheduleEntries.Add(entry);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }

            }, $"Error adding schedule entry for Class {schoolClassId}, Subject {subjectId}");
        }

        public async Task<List<ScheduleEntry>> GetScheduleForClassAsync(int schoolClassId)
        {
            return await ExecuteSafeAsync(async () =>
            {
                return await _context.ScheduleEntries
                    .Include(se => se.ClassSubject)
                        .ThenInclude(cs => cs.Subject)
                        .ThenInclude(s => s.Teacher)
                    .Include(se => se.ClassSubject)
                        .ThenInclude(cs => cs.SchoolClass)
                    .Where(se => se.SchoolClassId == schoolClassId)
                    .OrderBy(se => se.DayOfWeek)
                    .ThenBy(se => se.StartTime)
                    .ToListAsync();
            }, $"Error retrieving schedule for class {schoolClassId}", new List<ScheduleEntry>());
        }

        public async Task<List<ScheduleEntry>> GetScheduleForTeacherAsync(int teacherId)
        {
            return await ExecuteSafeAsync(async () =>
            {
                return await _context.ScheduleEntries
                    .Include(se => se.ClassSubject)
                        .ThenInclude(cs => cs.Subject)
                        .ThenInclude(s => s.Teacher)
                    .Include(se => se.ClassSubject)
                        .ThenInclude(cs => cs.SchoolClass)
                    .Where(se => se.ClassSubject.Subject.TeacherId == teacherId)
                    .OrderBy(se => se.DayOfWeek)
                    .ThenBy(se => se.StartTime)
                    .ToListAsync();
            }, $"Error retrieving schedule for teacher {teacherId}", new List<ScheduleEntry>());
        }
    }
}
