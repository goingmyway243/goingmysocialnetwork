namespace SocialNetworkMicroservices.Identity.Models;

public class PkceLoginRequest
{
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required string CodeChallenge { get; set; }
    public string? CodeChallengeMethod { get; set; } = "S256";
    public string? ClientId { get; set; }
    public string? RedirectUri { get; set; }
    public string? Scope { get; set; }
}
