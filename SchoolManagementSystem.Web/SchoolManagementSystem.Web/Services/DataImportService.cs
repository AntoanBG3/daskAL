using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Web.Data;
using SchoolManagementSystem.Web.DTOs;
using SchoolManagementSystem.Web.Models;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

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
            var schoolData = DeserializeSchoolData(jsonContent);

            if (schoolData is null)
                return;

            await ExecuteImportStepsInTransactionAsync(schoolData);
        }

        // ── Private import steps ───────────────────────────────────────────

        private LegacySchoolDatabase? DeserializeSchoolData(string jsonContent)
        {
            var schoolData = JsonSerializer.Deserialize<LegacySchoolDatabase>(jsonContent);

            if (schoolData is null)
                _logger.LogWarning("Deserialized school data was null.");

            return schoolData;
        }

        private async Task ExecuteImportStepsInTransactionAsync(LegacySchoolDatabase schoolData)
        {
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

        private async Task ImportTeachersAsync(LegacySchoolDatabase schoolData)
        {
            if (await _context.Teachers.AnyAsync() || schoolData.Teachers is null or { Count: 0 })
                return;

            var teachers = schoolData.Teachers.Select(t => new Teacher
            {
                FirstName = t.FirstName,
                LastName = t.LastName
            });

            _context.Teachers.AddRange(teachers);

            await _context.SaveChangesAsync();
            _logger.LogInformation("Imported {Count} teachers.", schoolData.Teachers.Count);
        }

        private async Task ImportSubjectsAsync(LegacySchoolDatabase schoolData)
        {
            if (await _context.Subjects.AnyAsync() || schoolData.Subjects is null or { Count: 0 })
                return;

            var teachers = await _context.Teachers.ToListAsync();
            var subjectTeacherIdByName = ResolveSubjectTeacherIds(schoolData.Teachers, teachers);

            var subjects = schoolData.Subjects.Select(s => new Subject
            {
                Name = s.Name,
                Description = s.Description,
                TeacherId = subjectTeacherIdByName.TryGetValue(s.Name, out var teacherId) ? teacherId : null
            });

            _context.Subjects.AddRange(subjects);

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

            _context.SchoolClasses.AddRange(legacyClasses.Select(className => new SchoolClass { Name = className }));

            await _context.SaveChangesAsync();
            _logger.LogInformation("Imported {Count} classes.", legacyClasses.Count);
        }

        private async Task ImportStudentsAsync(LegacySchoolDatabase schoolData)
        {
            if (await _context.Students.AnyAsync() || schoolData.Students is null or { Count: 0 })
                return;

            var classes = await _context.SchoolClasses.ToListAsync();
            var subjects = await _context.Subjects.ToListAsync();
            var classIdsByName = BuildIdLookup(classes, c => c.Name, c => c.Id);
            var subjectIdsByName = BuildIdLookup(subjects, s => s.Name, s => s.Id);

            foreach (var s in schoolData.Students)
            {
                var newStudent = new Student
                {
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    SchoolClassId = classIdsByName.TryGetValue(s.Class, out var classId) ? classId : null,
                    DateOfBirth = s.DateOfBirth
                };

                AddStudentGrades(newStudent, s.SubjectGrades, subjectIdsByName);

                _context.Students.Add(newStudent);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Imported {Count} students.", schoolData.Students.Count);
        }

        private static Dictionary<string, int?> ResolveSubjectTeacherIds(
            ICollection<LegacyTeacher>? legacyTeachers,
            IEnumerable<Teacher> teachers)
        {
            var teacherIdByName = teachers
                .GroupBy(t => (t.FirstName, t.LastName))
                .ToDictionary(g => g.Key, g => g.First().Id);

            var subjectTeacherIdByName = new Dictionary<string, int?>(StringComparer.Ordinal);
            if (legacyTeachers is null)
                return subjectTeacherIdByName;

            foreach (var legacyTeacher in legacyTeachers)
            {
                if (!teacherIdByName.TryGetValue((legacyTeacher.FirstName, legacyTeacher.LastName), out var teacherId))
                    continue;

                foreach (var subjectName in legacyTeacher.TeachingSubjects)
                {
                    if (!subjectTeacherIdByName.ContainsKey(subjectName))
                        subjectTeacherIdByName[subjectName] = teacherId;
                }
            }

            return subjectTeacherIdByName;
        }

        private static Dictionary<string, int> BuildIdLookup<TEntity>(
            IEnumerable<TEntity> entities,
            Func<TEntity, string> nameSelector,
            Func<TEntity, int> idSelector)
        {
            var result = new Dictionary<string, int>(StringComparer.Ordinal);

            foreach (var entity in entities)
            {
                var name = nameSelector(entity);
                if (!string.IsNullOrWhiteSpace(name) && !result.ContainsKey(name))
                    result[name] = idSelector(entity);
            }

            return result;
        }

        private static void AddStudentGrades(
            Student student,
            Dictionary<string, List<int>>? subjectGrades,
            IReadOnlyDictionary<string, int> subjectIdsByName)
        {
            if (subjectGrades is null)
                return;

            foreach (var subjectEntry in subjectGrades)
            {
                var subjectName = subjectEntry.Key;
                var subjectId = subjectIdsByName.TryGetValue(subjectName, out var mappedSubjectId)
                    ? mappedSubjectId
                    : (int?)null;

                foreach (var gradeValue in subjectEntry.Value)
                {
                    student.Grades.Add(new Grade
                    {
                        SubjectId = subjectId,
                        Value = Math.Clamp(gradeValue, 2, 6)
                    });
                }
            }
        }
    }
}
