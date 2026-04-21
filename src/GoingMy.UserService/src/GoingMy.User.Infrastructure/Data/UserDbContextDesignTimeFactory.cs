using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

namespace GoingMy.User.Infrastructure.Data;

/// <summary>
/// Provides a <see cref="UserDbContext"/> instance for EF Core design-time tools
/// (migrations, scaffolding) without requiring the full application DI container.
/// </summary>
public class UserDbContextDesignTimeFactory : IDesignTimeDbContextFactory<UserDbContext>
{
    public UserDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<UserDbContext>();

        // Use a placeholder connection string — only the schema matters for migrations
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=userdb;Username=postgres;Password=postgres");

        // Register the interceptor so the context is valid (no actual RabbitMQ calls at design time)
        var interceptor = new UserProfileOutboxInterceptor();
        optionsBuilder.AddInterceptors(interceptor);

        return new UserDbContext(optionsBuilder.Options);
    }
}
