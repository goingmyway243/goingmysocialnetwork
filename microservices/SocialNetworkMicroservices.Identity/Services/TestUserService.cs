using SocialNetworkMicroservices.Identity.Models;

namespace SocialNetworkMicroservices.Identity.Services;

public class TestUserService : IUserService
{
    private readonly List<TestUser> _testUsers = new()
    {
        new TestUser
        {
            Id = "1",
            Username = "admin",
            Password = "admin123",
            Email = "admin@socialnetwork.com",
            FirstName = "Admin",
            LastName = "User",
            Roles = new List<string> { "admin", "user" }
        },
        new TestUser
        {
            Id = "2",
            Username = "john.doe",
            Password = "password123",
            Email = "john.doe@example.com",
            FirstName = "John",
            LastName = "Doe",
            Roles = new List<string> { "user" }
        },
        new TestUser
        {
            Id = "3",
            Username = "jane.smith",
            Password = "password123",
            Email = "jane.smith@example.com",
            FirstName = "Jane",
            LastName = "Smith",
            Roles = new List<string> { "user" }
        },
        new TestUser
        {
            Id = "4",
            Username = "test",
            Password = "password",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            Roles = new List<string> { "user" }
        },
        new TestUser
        {
            Id = "5",
            Username = "moderator",
            Password = "mod123",
            Email = "moderator@socialnetwork.com",
            FirstName = "Moderator",
            LastName = "User",
            Roles = new List<string> { "moderator", "user" }
        }
    };

    public TestUser? ValidateCredentials(string username, string password)
    {
        return _testUsers.FirstOrDefault(u => 
            u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && 
            u.Password == password);
    }

    public TestUser? GetUserByUsername(string username)
    {
        return _testUsers.FirstOrDefault(u => 
            u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
    }

    public IEnumerable<TestUser> GetAllTestUsers()
    {
        return _testUsers.Select(u => new TestUser
        {
            Id = u.Id,
            Username = u.Username,
            Email = u.Email,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Roles = u.Roles,
            Password = "********" // Don't expose actual passwords
        });
    }
}
