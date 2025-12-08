using Microsoft.AspNetCore.Identity;
using SchoolManagementSystem.Web.Models.Auth;
using SchoolManagementSystem.Web.Models;

namespace SchoolManagementSystem.Web.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAndUsersAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<Role>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            var dbContext = serviceProvider.GetRequiredService<SchoolDbContext>();

            string[] roleNames = { "Admin", "Teacher", "Student" };

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new Role(roleName));
                }
            }

            // Seed Admin
            var adminEmail = "admin@school.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                var newAdmin = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "System",
                    LastName = "Admin",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                var result = await userManager.CreateAsync(newAdmin, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                }
            }

            // Seed Teacher
            var teacherEmail = "teacher@school.com";
            var teacherUser = await userManager.FindByEmailAsync(teacherEmail);
            if (teacherUser == null)
            {
                var newTeacher = new User
                {
                    UserName = teacherEmail,
                    Email = teacherEmail,
                    FirstName = "John",
                    LastName = "Doe",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                var result = await userManager.CreateAsync(newTeacher, "Teacher123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newTeacher, "Teacher");
                    
                    // Link to Teacher Entity
                    if (!dbContext.Teachers.Any(t => t.UserId == newTeacher.Id))
                    {
                        dbContext.Teachers.Add(new Teacher 
                        { 
                            FirstName = "John", 
                            LastName = "Doe", 
                            UserId = newTeacher.Id // Link
                        });
                        await dbContext.SaveChangesAsync();
                    }
                }
            }

            // Seed Student
            var studentEmail = "student@school.com";
            var studentUser = await userManager.FindByEmailAsync(studentEmail);
            if (studentUser == null)
            {
                var newStudent = new User
                {
                    UserName = studentEmail,
                    Email = studentEmail,
                    FirstName = "Jane",
                    LastName = "Smith",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                var result = await userManager.CreateAsync(newStudent, "Student123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newStudent, "Student");
                    
                     // Link to Student Entity
                     // Ensure a class exists first
                     var defaultClass = dbContext.SchoolClasses.FirstOrDefault(c => c.Name == "10A");
                     if (defaultClass == null)
                     {
                         defaultClass = new SchoolClass { Name = "10A" };
                         dbContext.SchoolClasses.Add(defaultClass);
                         await dbContext.SaveChangesAsync();
                     }

                    if (!dbContext.Students.Any(s => s.UserId == newStudent.Id))
                    {
                        dbContext.Students.Add(new Student 
                        { 
                            FirstName = "Jane", 
                            LastName = "Smith", 
                            DateOfBirth = new DateTime(2008, 1, 1),
                            SchoolClassId = defaultClass.Id,
                            UserId = newStudent.Id // Link
                        });
                        await dbContext.SaveChangesAsync();
                    }
                }
            }
        }
    }
}
