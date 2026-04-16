using StackExchange.Redis;

namespace GoingMy.Auth.API.Services;

public class RefreshTokenBlacklistService : IRefreshTokenBlacklistService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RefreshTokenBlacklistService> _logger;
    private const string BlacklistKeyPrefix = "token:blacklist:";

    public RefreshTokenBlacklistService(
        IConnectionMultiplexer redis,
        ILogger<RefreshTokenBlacklistService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task RevokeTokenAsync(string tokenJti, string subject, DateTime expiresAt, CancellationToken cancellationToken = default)
    {
        try
        {
            var key = $"{BlacklistKeyPrefix}{tokenJti}";
            var db = _redis.GetDatabase();

            // Calculate TTL based on token expiry
            var ttl = expiresAt - DateTime.UtcNow;

            // Store in Redis with automatic expiration
            // Redis will automatically delete the key after TTL expires
            await db.StringSetAsync(
                key,
                subject,
                expiry: ttl > TimeSpan.Zero ? ttl : TimeSpan.FromSeconds(1)
            );

            _logger.LogInformation("Refresh token revoked in Redis: {TokenJti}", tokenJti);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking refresh token in Redis: {TokenJti}", tokenJti);
            throw;
        }
    }

    public async Task<bool> IsTokenRevokedAsync(string tokenJti, CancellationToken cancellationToken = default)
    {
        try
        {
            var key = $"{BlacklistKeyPrefix}{tokenJti}";
            var db = _redis.GetDatabase();

            // Check if token exists in blacklist
            // Returns true if the key exists (token is blacklisted)
            var result = await db.KeyExistsAsync(key);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if token is revoked in Redis: {TokenJti}", tokenJti);
            throw;
        }
    }
}
