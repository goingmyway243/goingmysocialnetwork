namespace GoingMy.Auth.API.Data;

/// <summary>
/// Represents a blacklisted refresh token that has been revoked on logout.
/// </summary>
public class RefreshTokenBlacklist
{
    /// <summary>
    /// The JWT ID (jti) claim of the refresh token. Used as the primary key.
    /// </summary>
    public string TokenJti { get; set; } = string.Empty;

    /// <summary>
    /// The timestamp when the refresh token was issued.
    /// </summary>
    public DateTime IssuedAt { get; set; }

    /// <summary>
    /// The timestamp when the refresh token expires.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// The subject (user ID) of the refresh token.
    /// </summary>
    public string Subject { get; set; } = string.Empty;
}
