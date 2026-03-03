namespace SocialNetworkMicroservices.Identity.Models;

public class LoginResponse
{
    public bool Success { get; set; }
    public string? AccessToken { get; set; }
    public string? TokenType { get; set; } = "Bearer";
    public int? ExpiresIn { get; set; }
    public string? RefreshToken { get; set; }
    public UserInfo? User { get; set; }
    public string? Error { get; set; }
    public string? ErrorDescription { get; set; }
}

public class UserInfo
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
}
