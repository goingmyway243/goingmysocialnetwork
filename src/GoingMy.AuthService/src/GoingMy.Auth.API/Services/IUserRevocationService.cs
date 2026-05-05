namespace GoingMy.Auth.API.Services;

public interface IUserRevocationService
{
    /// <summary>
    /// Records the current UTC timestamp as the revocation point for all tokens issued to the given user.
    /// Any access token with an 'iat' (issued-at) equal to or before this timestamp will be rejected by the gateway.
    /// The Redis key is stored with a 30-day TTL, covering all plausible token lifetimes.
    /// </summary>
    Task RevokeUserTokensAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the Unix timestamp (seconds) stored as the revocation point for the user,
    /// or null if no revocation has been issued.
    /// </summary>
    Task<long?> GetRevocationTimestampAsync(string userId, CancellationToken cancellationToken = default);
}
