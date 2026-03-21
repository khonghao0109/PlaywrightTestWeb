using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using MyWebApp.Models;

namespace MyWebApp.Repository
{
    public class SeedData
    {
        // Existing product seeding commented out here

        /// <summary>
        /// Seed an admin role and an admin user if they do not exist.
        /// Call this during application startup: await SeedData.SeedAdminAsync(app.Services);
        /// </summary>
        public static async Task SeedAdminAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;

            var userManager = services.GetRequiredService<UserManager<AppUserModel>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            string adminRoleName = "Admin";
            // create role if not exists
            var adminRole = await roleManager.FindByNameAsync(adminRoleName);
            if (adminRole == null)
            {
                adminRole = new IdentityRole(adminRoleName);
                await roleManager.CreateAsync(adminRole);
            }

            // admin account info - change these values for production
            string adminUserName = "Admin01";
            string adminEmail = "admin@mywebapp.local";
            string adminPassword = "Admin@12345"; // replace with a secure password or read from config

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new AppUserModel
                {
                    UserName = adminUserName,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    Occupation = "Administrator",
                    RoleId = adminRole.Id
                };

                var createResult = await userManager.CreateAsync(adminUser, adminPassword);
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, adminRoleName);
                    adminUser.RoleId = adminRole.Id;
                    await userManager.UpdateAsync(adminUser);
                }
                else
                {
                    // Optionally log or throw an exception in real apps
                }
            }
            else
            {
                // Ensure user is in role and RoleId is set
                var inRole = await userManager.IsInRoleAsync(adminUser, adminRoleName);
                if (!inRole)
                {
                    await userManager.AddToRoleAsync(adminUser, adminRoleName);
                }
                if (adminUser.RoleId != adminRole.Id)
                {
                    adminUser.RoleId = adminRole.Id;
                    await userManager.UpdateAsync(adminUser);
                }
            }
        }
    }
}
