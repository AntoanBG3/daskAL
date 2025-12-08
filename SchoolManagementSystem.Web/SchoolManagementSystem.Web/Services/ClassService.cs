using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Web.Data;
using SchoolManagementSystem.Web.Models;
using SchoolManagementSystem.Web.Models.ViewModels;

namespace SchoolManagementSystem.Web.Services
{
    public class ClassService
    {
        private readonly SchoolDbContext _context;
        private readonly ILogger<ClassService> _logger;

        public ClassService(SchoolDbContext context, ILogger<ClassService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<SchoolClassViewModel>> GetAllClassesAsync()
        {
            try
            {
                var classes = await _context.SchoolClasses
                    .Include(c => c.ClassSubjects)
                    .ThenInclude(cs => cs.Subject)
                    .ToListAsync();

                return classes.Select(c => new SchoolClassViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    AssignedSubjects = c.ClassSubjects.Select(cs => cs.Subject.Name).ToList()
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all classes.");
                return new List<SchoolClassViewModel>();
            }
        }

        public async Task<SchoolClassViewModel?> GetClassByIdAsync(int id)
        {
            try
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
                    AssignedSubjects = c.ClassSubjects.Select(cs => cs.Subject.Name).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving class with ID {ClassId}", id);
                return null;
            }
        }

        public async Task AddClassAsync(SchoolClassViewModel model)
        {
            try
            {
                var schoolClass = new SchoolClass { Name = model.Name };
                _context.SchoolClasses.Add(schoolClass);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding class {ClassName}", model.Name);
                throw;
            }
        }

        public async Task UpdateClassAsync(SchoolClassViewModel model)
        {
            try
            {
                var schoolClass = await _context.SchoolClasses.FindAsync(model.Id);
                if (schoolClass != null)
                {
                    schoolClass.Name = model.Name;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating class {ClassId}", model.Id);
                throw;
            }
        }

        public async Task UpdateClassSubjectsAsync(int classId, List<int> subjectIds)
        {
            try
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating subjects for class {ClassId}", classId);
                throw;
            }
        }
    }
}
