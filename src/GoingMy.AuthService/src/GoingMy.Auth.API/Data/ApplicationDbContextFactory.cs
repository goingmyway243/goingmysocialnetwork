using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GoingMy.Auth.API.Data;

/// <summary>
/// Design-time factory for ApplicationDbContext to support EF Core migrations.
/// This is only used by EF Core tools (dotnet ef migrations), not at runtime.
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Create DbContextOptions with Npgsql provider
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql("");

        // Optional: Enable sensitive data logging for development
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.EnableDetailedErrors();

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
