using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Web.Data;
using SchoolManagementSystem.Web.Models;
using SchoolManagementSystem.Web.Models.ViewModels;

namespace SchoolManagementSystem.Web.Services
{
    public class ClassService : BaseService<ClassService>
    {
        private readonly SchoolDbContext _context;

        public ClassService(SchoolDbContext context, ILogger<ClassService> logger) : base(logger)
        {
            _context = context;
        }

        public async Task<List<SchoolClassViewModel>> GetAllClassesAsync()
        {
            return await ExecuteSafeAsync(async () =>
            {
                var classes = await _context.SchoolClasses
                    .Include(c => c.ClassSubjects)
                    .ThenInclude(cs => cs.Subject)
                    .ToListAsync();

                return classes.Select(c => new SchoolClassViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    AssignedSubjects = c.ClassSubjects.Select(cs => cs.Subject?.Name ?? "Unknown").ToList()
                }).ToList();
            }, "Error occurred while retrieving all classes.", new List<SchoolClassViewModel>());
        }

        public async Task<SchoolClassViewModel?> GetClassByIdAsync(int id)
        {
            return await ExecuteSafeAsync(async () =>
            {
                var c = await _context.SchoolClasses
                    .Include(c => c.ClassSubjects)
                    .ThenInclude(cs => cs.Subject)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (c == null) return null;

                return new SchoolClassViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    AssignedSubjects = c.ClassSubjects.Select(cs => cs.Subject?.Name ?? "Unknown").ToList()
                };
            }, $"Error occurred while retrieving class with ID {id}", null);
        }

        public async Task AddClassAsync(SchoolClassViewModel model)
        {
            await ExecuteSafeAsync(async () =>
            {
                var schoolClass = new SchoolClass { Name = model.Name };
                _context.SchoolClasses.Add(schoolClass);
                await _context.SaveChangesAsync();
            }, $"Error occurred while adding class {model.Name}");
        }

        public async Task UpdateClassAsync(SchoolClassViewModel model)
        {
            await ExecuteSafeAsync(async () =>
            {
                var schoolClass = await _context.SchoolClasses.FindAsync(model.Id);
                if (schoolClass != null)
                {
                    schoolClass.Name = model.Name;
                    await _context.SaveChangesAsync();
                }
            }, $"Error occurred while updating class {model.Id}");
        }

        public async Task UpdateClassSubjectsAsync(int classId, List<int> subjectIds)
        {
            await ExecuteSafeAsync(async () =>
            {
                var schoolClass = await _context.SchoolClasses
                    .Include(c => c.ClassSubjects)
                    .FirstOrDefaultAsync(c => c.Id == classId);

                if (schoolClass != null)
                {
                    // Remove existing
                    _context.ClassSubjects.RemoveRange(schoolClass.ClassSubjects);
                    
                    // Add new
                    foreach (var subjectId in subjectIds)
                    {
                        _context.ClassSubjects.Add(new ClassSubject { SchoolClassId = classId, SubjectId = subjectId });
                    }
                    
                    await _context.SaveChangesAsync();
                }
            }, $"Error occurred while updating subjects for class {classId}");
        }
    }
}
