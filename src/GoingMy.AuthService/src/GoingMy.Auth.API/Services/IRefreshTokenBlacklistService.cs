namespace GoingMy.Auth.API.Services;

public interface IRefreshTokenBlacklistService
{
    /// <summary>
    /// Revokes a refresh token by adding it to the Redis blacklist.
    /// The token will automatically expire from Redis based on the provided expiration time.
    /// </summary>
    Task RevokeTokenAsync(string tokenJti, string subject, DateTime expiresAt, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a refresh token has been revoked (is in the Redis blacklist).
    /// </summary>
    Task<bool> IsTokenRevokedAsync(string tokenJti, CancellationToken cancellationToken = default);
}
