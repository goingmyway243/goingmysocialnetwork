using Microsoft.EntityFrameworkCore;
using SocialNetworkMicroservices.Identity.Entities;

namespace SocialNetworkMicroservices.Identity.Database;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .Property(u => u.FullName)
            .HasMaxLength(100);

        modelBuilder.Entity<User>()
            .Property(u => u.ProfilePicture)
            .HasMaxLength(255);

        modelBuilder.Entity<User>()
            .Property(u => u.Location)
            .HasMaxLength(255);

        modelBuilder.Entity<User>()
            .Property(u => u.Website)
            .HasMaxLength(255);

        modelBuilder.Entity<User>()
            .Property(u => u.Gender)
            .HasMaxLength(10);
    }
}
