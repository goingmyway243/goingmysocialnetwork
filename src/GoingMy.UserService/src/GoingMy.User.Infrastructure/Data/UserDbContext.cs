using GoingMy.User.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GoingMy.User.Infrastructure.Data;

public class UserDbContext(DbContextOptions<UserDbContext> options) : DbContext(options)
{
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<UserFollow> UserFollows => Set<UserFollow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.ToTable("UserProfiles");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever(); // ID comes from AuthService
            entity.Property(e => e.Username).IsRequired().HasMaxLength(256);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Bio).HasMaxLength(500);
            entity.Property(e => e.Location).HasMaxLength(100);
            entity.Property(e => e.WebsiteUrl).HasMaxLength(200);
            entity.Property(e => e.Gender).HasConversion<string>();
            entity.HasIndex(e => e.Username).IsUnique();
        });

        modelBuilder.Entity<UserFollow>(entity =>
        {
            entity.ToTable("UserFollows");
            entity.HasKey(e => new { e.FollowerId, e.FolloweeId });
            entity.HasIndex(e => e.FolloweeId);
            entity.HasIndex(e => e.FollowerId);
        });
    }
}
