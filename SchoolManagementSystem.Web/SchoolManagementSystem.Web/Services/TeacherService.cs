using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Web.Data;
using SchoolManagementSystem.Web.Models;
using SchoolManagementSystem.Web.Models.ViewModels;

namespace SchoolManagementSystem.Web.Services
{
    public class TeacherService : BaseService<TeacherService>
    {
        private readonly SchoolDbContext _context;
        private readonly Microsoft.AspNetCore.Identity.UserManager<SchoolManagementSystem.Web.Models.Auth.User> _userManager;

        public TeacherService(SchoolDbContext context, Microsoft.AspNetCore.Identity.UserManager<SchoolManagementSystem.Web.Models.Auth.User> userManager, ILogger<TeacherService> logger) : base(logger)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<List<TeacherViewModel>> GetAllTeachersAsync()
        {
            return await ExecuteSafeAsync(async () =>
            {
                var teachers = await _context.Teachers.Include(t => t.TeachingSubjects).ToListAsync();
                return teachers.Select(t => new TeacherViewModel
                {
                    Id = t.Id,
                    FirstName = t.FirstName,
                    LastName = t.LastName,
                    TeachingSubjects = t.TeachingSubjects.Select(s => s.Name).ToList()
                }).ToList();
            }, "Error occurred while retrieving all teachers.", new List<TeacherViewModel>());
        }

        public async Task<TeacherViewModel?> GetTeacherByIdAsync(int id)
        {
            return await ExecuteSafeAsync(async () =>
            {
                var t = await _context.Teachers.Include(x => x.TeachingSubjects).FirstOrDefaultAsync(x => x.Id == id);
                if (t == null) return null;

                var email = "";
                if (!string.IsNullOrEmpty(t.UserId))
                {
                    var user = await _userManager.FindByIdAsync(t.UserId);
                    if (user != null)
                    {
                        email = user.Email ?? "";
                    }
                }
                
                return new TeacherViewModel
                {
                    Id = t.Id,
                    FirstName = t.FirstName,
                    LastName = t.LastName,
                    Email = email,
                    UserId = t.UserId,
                    TeachingSubjects = t.TeachingSubjects.Select(s => s.Name).ToList()
                };
            }, $"Error occurred while retrieving teacher with ID {id}", null);
        }

        public async Task AddTeacherAsync(TeacherViewModel model)
        {
            await AddTeacherAsync(model, null);
        }

        public async Task AddTeacherAsync(TeacherViewModel model, string? userId)
        {
            await ExecuteSafeAsync(async () =>
            {
                var teacher = new Teacher
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    UserId = userId
                };
                _context.Teachers.Add(teacher);
                await _context.SaveChangesAsync();
            }, $"Error occurred while adding teacher {model.FirstName} {model.LastName}");
        }

        public async Task UpdateTeacherAsync(TeacherViewModel model)
        {
            await ExecuteSafeAsync(async () =>
            {
                var teacher = await _context.Teachers.FindAsync(model.Id);
                if (teacher != null)
                {
                    teacher.FirstName = model.FirstName;
                    teacher.LastName = model.LastName;
                    
                    if (!string.IsNullOrEmpty(teacher.UserId))
                    {
                         var user = await _userManager.FindByIdAsync(teacher.UserId);
                         if (user != null)
                         {
                             // Update Email (and Username as they should be sync)
                             if (!string.Equals(user.Email, model.Email, StringComparison.OrdinalIgnoreCase))
                             {
                                 user.Email = model.Email;
                                 user.UserName = model.Email;
                                 await _userManager.UpdateAsync(user);
                             }

                             // Update Password if provided
                             if (!string.IsNullOrWhiteSpace(model.Password))
                             {
                                 var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                                 var result = await _userManager.ResetPasswordAsync(user, token, model.Password);
                                 if (!result.Succeeded)
                                 {
                                     throw new Exception($"Failed to reset password: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                                 }
                             }
                         }
                    }
                    else if (!string.IsNullOrEmpty(model.Email) && !string.IsNullOrEmpty(model.Password))
                    {
                        // Create new User
                         var user = new SchoolManagementSystem.Web.Models.Auth.User
                         {
                             UserName = model.Email,
                             Email = model.Email,
                             FirstName = model.FirstName,
                             LastName = model.LastName,
                             CreatedAt = DateTime.UtcNow,
                             IsActive = true // Active by default when created by Admin
                         };

                         var result = await _userManager.CreateAsync(user, model.Password);
                         if (result.Succeeded)
                         {
                             await _userManager.AddToRoleAsync(user, "Teacher");
                             teacher.UserId = user.Id;
                         }
                         else
                         {
                              throw new Exception($"Failed to create user account: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                         }
                    }

                    await _context.SaveChangesAsync();
                }
            }, $"Error occurred while updating teacher {model.Id}");
        }
        
        public async Task DeleteTeacherAsync(int id)
        {
            await ExecuteSafeAsync(async () =>
            {
                var teacher = await _context.Teachers.FindAsync(id);
                if (teacher != null)
                {
                    _context.Teachers.Remove(teacher);
                    await _context.SaveChangesAsync();
                }
            }, $"Error occurred while deleting teacher {id}");
        }

        public async Task<Teacher?> GetTeacherByUserIdAsync(string userId)
        {
            return await ExecuteSafeAsync(async () =>
            {
                return await _context.Teachers
                    .FirstOrDefaultAsync(t => t.UserId == userId);
            }, $"Error occurred while retrieving teacher by UserID {userId}", null);
        }
    }
}
