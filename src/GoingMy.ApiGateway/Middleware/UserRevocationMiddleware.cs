using StackExchange.Redis;
using System.Security.Claims;

namespace GoingMy.ApiGateway.Middleware;

/// <summary>
/// Checks whether the authenticated user's current token was issued before an admin-triggered revocation.
/// If the Redis key "user:{userId}:tokens_revoked_at" exists and the token's "iat" (issued-at Unix timestamp)
/// is less than or equal to the stored revocation timestamp, the request is rejected with 401.
/// New tokens issued after revocation have a higher "iat" and are accepted normally.
/// </summary>
public class UserRevocationMiddleware(
    RequestDelegate next,
    IConnectionMultiplexer redis,
    ILogger<UserRevocationMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var user = context.User;
        if (user?.Identity?.IsAuthenticated == true)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? user.FindFirstValue("sub");

            if (userId is not null)
            {
                var db = redis.GetDatabase();
                var revokedAtValue = await db.StringGetAsync($"user:{userId}:tokens_revoked_at");

                if (!revokedAtValue.IsNullOrEmpty && long.TryParse((string?)revokedAtValue, out var revokedAtTs))
                {
                    // Try standard "iat" claim (Unix timestamp seconds)
                    var iatStr = user.FindFirstValue("iat");

                    if (long.TryParse(iatStr, out var tokenIat) && tokenIat > revokedAtTs)
                    {
                        // Token was issued after revocation — allow through
                        await next(context);
                        return;
                    }

                    // Token issued before/at revocation, or iat not parseable — reject
                    logger.LogWarning(
                        "Blocked revoked token for user {UserId} (token iat={Iat}, revoked_at={RevokedAt})",
                        userId, iatStr, revokedAtTs);

                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }
            }
        }

        await next(context);
    }
}
