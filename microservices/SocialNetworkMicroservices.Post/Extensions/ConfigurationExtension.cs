namespace SocialNetworkMicroservices.Post.Extensions;

public static class ConfigurationExtension
{
    public static string? GetServiceUri(this IConfiguration config, string appName)
    {
        string? serviceDiscoveryUrl = config[$"services:{appName.ToLowerInvariant()}:https:0"];
        return serviceDiscoveryUrl;
    }
}
