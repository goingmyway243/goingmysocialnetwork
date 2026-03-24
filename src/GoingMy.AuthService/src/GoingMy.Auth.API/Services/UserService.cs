using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GoingMy.Auth.API.Data;
using GoingMy.Auth.API.Dtos;
using GoingMy.Auth.API.Enums;
using GoingMy.Auth.API.Models;

namespace GoingMy.Auth.API.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<ApplicationUser?> ValidateCredentialsAsync(string username, string password)
    {
        var user = await _userManager.FindByNameAsync(username);
        if (user == null || !user.IsActive)
        {
            return null;
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);
        return isPasswordValid ? user : null;
    }

    public async Task<ApplicationUser?> GetUserByUsernameAsync(string username)
    {
        return await _userManager.FindByNameAsync(username);
    }

    public async Task<ApplicationUser?> GetUserByIdAsync(string userId)
    {
        return await _userManager.FindByIdAsync(userId);
    }

    public async Task<ApplicationUser> CreateUserAsync(
        string username, 
        string password, 
        string email, 
        string firstName, 
        string lastName, 
        List<string> roles)
    {
        var existingUser = await _userManager.FindByNameAsync(username);
        if (existingUser != null)
        {
            throw new InvalidOperationException($"User with username '{username}' already exists.");
        }

        var existingEmail = await _userManager.FindByEmailAsync(email);
        if (existingEmail != null)
        {
            throw new InvalidOperationException($"User with email '{email}' already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = username,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Roles = roles.Select(r => Enum.Parse<UserRole>(r)).ToList(),
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create user: {errors}");
        }

        return user;
    }

    public async Task<IEnumerable<ApplicationUser>> GetAllUsersAsync()
    {
        return await _context.Users
            .Where(u => u.IsActive)
            .ToListAsync();
    }

    public async Task<ApplicationUser> UpdateUserAsync(string userId, UpdateUserDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID '{userId}' not found.");
        }

        // Apply updates from DTO - only update if value is provided
        if (!string.IsNullOrWhiteSpace(dto.FirstName))
            user.FirstName = dto.FirstName;

        if (!string.IsNullOrWhiteSpace(dto.LastName))
            user.LastName = dto.LastName;

        if (dto.Bio is not null)
            user.Bio = dto.Bio;

        if (dto.DateOfBirth.HasValue)
            user.DateOfBirth = dto.DateOfBirth.Value;

        if (dto.Gender.HasValue)
            user.Gender = dto.Gender.Value;

        if (dto.Location is not null)
            user.Location = dto.Location;

        if (dto.WebsiteUrl is not null)
            user.WebsiteUrl = dto.WebsiteUrl;

        if (dto.IsPrivate.HasValue)
            user.IsPrivate = dto.IsPrivate.Value;

        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to update user: {errors}");
        }

        return user;
    }

    public async Task DeactivateUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID '{userId}' not found.");
        }

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to deactivate user: {errors}");
        }
    }
}
