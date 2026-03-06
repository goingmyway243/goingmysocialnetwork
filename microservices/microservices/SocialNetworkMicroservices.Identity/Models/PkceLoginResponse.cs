namespace SocialNetworkMicroservices.Identity.Models;

public class PkceLoginResponse
{
    public bool Success { get; set; }
    public string? AuthorizationCode { get; set; }
    public string? RedirectUri { get; set; }
    public UserInfo? User { get; set; }
    public string? Error { get; set; }
    public string? ErrorDescription { get; set; }
}
