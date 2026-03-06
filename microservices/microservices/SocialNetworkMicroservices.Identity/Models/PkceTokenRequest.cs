namespace SocialNetworkMicroservices.Identity.Models;

public class PkceTokenRequest
{
    public required string Code { get; set; }
    public required string CodeVerifier { get; set; }
    public string? ClientId { get; set; }
    public string? RedirectUri { get; set; }
}
