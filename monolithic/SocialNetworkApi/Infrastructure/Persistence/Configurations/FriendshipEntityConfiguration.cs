using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialNetworkApi.Domain.Entities;

namespace SocialNetworkApi.Infrastructure.Persistence.Configurations;

public class FriendshipEntityConfiguration : IEntityTypeConfiguration<FriendshipEntity>
{
    public void Configure(EntityTypeBuilder<FriendshipEntity> builder)
    {
        builder.Property(f => f.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasIndex(f => new { f.UserId, f.FriendId })
            .IsUnique()
            .HasDatabaseName("IX_Friendship_UserId_FriendId");
    }
}
