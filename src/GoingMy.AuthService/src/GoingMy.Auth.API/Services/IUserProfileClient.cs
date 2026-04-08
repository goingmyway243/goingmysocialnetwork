namespace GoingMy.Auth.API.Services;

/// <summary>
/// HTTP client interface for calling the UserService to bootstrap a user profile
/// immediately after AuthService creates a new account.
/// </summary>
public interface IUserProfileClient
{
    /// <summary>Creates a minimal user profile in UserService.</summary>
    Task CreateProfileAsync(Guid id, string username, string firstName, string lastName, CancellationToken ct = default);
}
