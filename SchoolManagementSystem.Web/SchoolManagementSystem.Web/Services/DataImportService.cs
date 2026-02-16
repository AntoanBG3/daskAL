using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Web.Data;
using SchoolManagementSystem.Web.DTOs;
using SchoolManagementSystem.Web.Models;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace SchoolManagementSystem.Web.Services
{
    public class DataImportService : BaseService<DataImportService>, IDataImportService
    {
        private readonly SchoolDbContext _context;

        public DataImportService(SchoolDbContext context, ILogger<DataImportService> logger)
            : base(logger)
        {
            _context = context;
        }

        public async Task ImportFromLegacyJsonAsync(string jsonPath)
        {
            if (!File.Exists(jsonPath))
            {
                _logger.LogWarning("Import file not found: {JsonPath}", jsonPath);
                return;
            }

            try
            {
                var json = await File.ReadAllTextAsync(jsonPath);
                await ImportFromJsonAsync(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading or parsing file: {JsonPath}", jsonPath);
            }
        }

        public async Task ImportFromJsonAsync(string jsonContent)
        {
            var schoolData = JsonSerializer.Deserialize<LegacySchoolDatabase>(jsonContent);

            if (schoolData is null)
            {
                _logger.LogWarning("Deserialized school data was null.");
                return;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                await ImportTeachersAsync(schoolData);
                await ImportSubjectsAsync(schoolData);
                await ImportClassesAsync(schoolData);
                await ImportStudentsAsync(schoolData);

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error processing imported data. Transaction rolled back.");
                throw;
            }
        }

        // ── Private import steps ───────────────────────────────────────────

        private async Task ImportTeachersAsync(LegacySchoolDatabase schoolData)
        {
            if (await _context.Teachers.AnyAsync() || schoolData.Teachers is null or { Count: 0 })
                return;

            foreach (var t in schoolData.Teachers)
            {
                _context.Teachers.Add(new Teacher
                {
                    FirstName = t.FirstName,
                    LastName = t.LastName
                });
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Imported {Count} teachers.", schoolData.Teachers.Count);
        }

        private async Task ImportSubjectsAsync(LegacySchoolDatabase schoolData)
        {
            if (await _context.Subjects.AnyAsync() || schoolData.Subjects is null or { Count: 0 })
                return;

            var teachers = await _context.Teachers.ToListAsync();

            foreach (var s in schoolData.Subjects)
            {
                var teacher = teachers.FirstOrDefault(t =>
                    schoolData.Teachers != null &&
                    schoolData.Teachers.Any(lt =>
                        lt.FirstName == t.FirstName &&
                        lt.LastName == t.LastName &&
                        lt.TeachingSubjects.Contains(s.Name)));

                _context.Subjects.Add(new Subject
                {
                    Name = s.Name,
                    Description = s.Description,
                    TeacherId = teacher?.Id
                });
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Imported {Count} subjects.", schoolData.Subjects.Count);
        }

        private async Task ImportClassesAsync(LegacySchoolDatabase schoolData)
        {
            if (await _context.SchoolClasses.AnyAsync() || schoolData.Students is null or { Count: 0 })
                return;

            var legacyClasses = schoolData.Students
                .Select(s => s.Class)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct()
                .ToList();

            foreach (var className in legacyClasses)
            {
                _context.SchoolClasses.Add(new SchoolClass { Name = className });
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Imported {Count} classes.", legacyClasses.Count);
        }

        private async Task ImportStudentsAsync(LegacySchoolDatabase schoolData)
        {
            if (await _context.Students.AnyAsync() || schoolData.Students is null or { Count: 0 })
                return;

            var classes = await _context.SchoolClasses.ToListAsync();
            var subjects = await _context.Subjects.ToListAsync(); // Queried once, outside the loop

            foreach (var s in schoolData.Students)
            {
                var schoolClass = classes.FirstOrDefault(c => c.Name == s.Class);

                var newStudent = new Student
                {
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    SchoolClassId = schoolClass?.Id,
                    DateOfBirth = s.DateOfBirth
                };

                if (s.SubjectGrades != null)
                {
                    foreach (var subjectName in s.SubjectGrades.Keys)
                    {
                        var subjectEntity = subjects.FirstOrDefault(sub => sub.Name == subjectName);

                        foreach (var val in s.SubjectGrades[subjectName])
                        {
                            newStudent.Grades.Add(new Grade
                            {
                                SubjectName = subjectName,
                                SubjectId = subjectEntity?.Id,
                                Value = val
                            });
                        }
                    }
                }

                _context.Students.Add(newStudent);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Imported {Count} students.", schoolData.Students.Count);
        }
    }
}
