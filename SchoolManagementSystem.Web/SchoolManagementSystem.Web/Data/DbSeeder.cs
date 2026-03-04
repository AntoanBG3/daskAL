using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
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
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

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
                var adminPassword = configuration["AdminSettings:DefaultPassword"] ?? "Admin123!";
                var result = await userManager.CreateAsync(newAdmin, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                }
            }

            // Do NOT seed Teacher and Student accounts as per requirements (Clean Slate).
        }
    }
}
