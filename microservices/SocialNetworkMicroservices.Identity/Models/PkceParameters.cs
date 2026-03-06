namespace SocialNetworkMicroservices.Identity.Models;

public class PkceParameters
{
    public required string CodeVerifier { get; set; }
    public required string CodeChallenge { get; set; }
    public required string CodeChallengeMethod { get; set; }
}
