using SocialNetworkMicroservices.Identity.Models;

namespace SocialNetworkMicroservices.Identity.Services;

public interface IUserService
{
    TestUser? ValidateCredentials(string username, string password);
    TestUser? GetUserByUsername(string username);
    IEnumerable<TestUser> GetAllTestUsers();
}
