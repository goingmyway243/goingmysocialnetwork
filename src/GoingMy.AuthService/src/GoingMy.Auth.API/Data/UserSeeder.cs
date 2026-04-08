using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GoingMy.Auth.API.Enums;
using GoingMy.Auth.API.Models;

namespace GoingMy.Auth.API.Data;

public static class UserSeeder
{
    public static async Task SeedUsersAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<ApplicationUser>>();

        // Check if admin user already exist
        if (await context.Users.FirstOrDefaultAsync(p => p.UserName == "admin") != null)
        {
            return; // Admin user already seeded
        }

        var user = new ApplicationUser
        {
            Id = Guid.Empty,
            UserName = "admin",
            NormalizedUserName = "ADMIN",
            Email = "admin@socialnetwork.com",
            NormalizedEmail = "ADMIN@SOCIALNETWORK.COM",
            FirstName = "Admin",
            LastName = "User",
            // Bio, IsVerified and other profile fields moved to UserService.
            Roles = [UserRole.Admin],
            SecurityStamp = Guid.NewGuid().ToString()
        };

        user.PasswordHash = passwordHasher.HashPassword(user, "admin123"); // Hash the password

        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
    }
}
