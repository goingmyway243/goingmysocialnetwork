using GoingMy.Auth.API.Dtos;
using GoingMy.Auth.API.Models;

namespace GoingMy.Auth.API.Services;

public interface IUserService
{
    Task<ApplicationUser?> ValidateCredentialsAsync(string username, string password);
    Task<ApplicationUser?> GetUserByUsernameAsync(string username);
    Task<ApplicationUser?> GetUserByIdAsync(string userId);
    Task<ApplicationUser> CreateUserAsync(string username, string password, string email, string firstName, string lastName, List<string> roles);
    Task<IEnumerable<ApplicationUser>> GetAllUsersAsync();
    Task<ApplicationUser> UpdateUserAsync(string userId, UpdateUserDto dto);
    Task DeactivateUserAsync(string userId);
}
