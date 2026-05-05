using StackExchange.Redis;

namespace GoingMy.Auth.API.Services;

public class UserRevocationService : IUserRevocationService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<UserRevocationService> _logger;
    private const string KeyPrefix = "user:";
    private const string KeySuffix = ":tokens_revoked_at";
    private static readonly TimeSpan Ttl = TimeSpan.FromDays(30);

    public UserRevocationService(IConnectionMultiplexer redis, ILogger<UserRevocationService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task RevokeUserTokensAsync(string userId, CancellationToken cancellationToken = default)
    {
        var key = $"{KeyPrefix}{userId}{KeySuffix}";
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var db = _redis.GetDatabase();
        await db.StringSetAsync(key, timestamp, expiry: Ttl);
        _logger.LogInformation("Revoked all tokens for user {UserId} at Unix ts {Timestamp}", userId, timestamp);
    }

    public async Task<long?> GetRevocationTimestampAsync(string userId, CancellationToken cancellationToken = default)
    {
        var key = $"{KeyPrefix}{userId}{KeySuffix}";
        var db = _redis.GetDatabase();
        var value = await db.StringGetAsync(key);
        if (value.IsNullOrEmpty) return null;
        return (long)value;
    }
}
