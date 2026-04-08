using System.Text;
using System.Text.Json;

namespace GoingMy.Auth.API.Services;

public class UserProfileClient(HttpClient httpClient, ILogger<UserProfileClient> logger) : IUserProfileClient
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public async Task CreateProfileAsync(
        Guid id,
        string username,
        string firstName,
        string lastName,
        CancellationToken ct = default)
    {
        var payload = new
        {
            id,
            username,
            firstName,
            lastName
        };

        var json = JsonSerializer.Serialize(payload, _jsonOptions);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await httpClient.PostAsync("api/userprofiles", content, ct);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(ct);
                logger.LogWarning(
                    "UserService returned {StatusCode} when bootstrapping profile for user {UserId}: {Body}",
                    (int)response.StatusCode, id, body);
            }
        }
        catch (Exception ex)
        {
            // Fire-and-forget: sign-up succeeds even if UserService is temporarily unavailable.
            // A retry / outbox pattern should be used in production.
            logger.LogError(ex, "Failed to bootstrap UserService profile for user {UserId}. Profile will be missing.", id);
        }
    }
}
