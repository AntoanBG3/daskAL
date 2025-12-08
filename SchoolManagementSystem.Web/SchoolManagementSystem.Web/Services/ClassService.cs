using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Web.Data;
using SchoolManagementSystem.Web.Models;
using SchoolManagementSystem.Web.Models.ViewModels;

namespace SchoolManagementSystem.Web.Services
{
    public class ClassService
    {
        private readonly SchoolDbContext _context;

        public ClassService(SchoolDbContext context)
        {
            _context = context;
        }

        public async Task<List<SchoolClassViewModel>> GetAllClassesAsync()
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

        public async Task<SchoolClassViewModel?> GetClassByIdAsync(int id)
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

        public async Task AddClassAsync(SchoolClassViewModel model)
        {
            var schoolClass = new SchoolClass { Name = model.Name };
            _context.SchoolClasses.Add(schoolClass);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateClassAsync(SchoolClassViewModel model)
        {
            var schoolClass = await _context.SchoolClasses.FindAsync(model.Id);
            if (schoolClass != null)
            {
                schoolClass.Name = model.Name;
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateClassSubjectsAsync(int classId, List<int> subjectIds)
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
    }
}
