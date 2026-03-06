using System.Text.Json.Serialization;

namespace SocialNetworkMicroservices.Identity.Models;

internal class AuthorizationResponse
{
    [JsonPropertyName("code")]
    public string? Code { get; set; }
    
    [JsonPropertyName("state")]
    public string? State { get; set; }
}
