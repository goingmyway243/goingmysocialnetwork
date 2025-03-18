using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialNetworkApi.Domain.Entities;

namespace SocialNetworkApi.Infrastructure.Persistence.Configurations;

public class UserEntityConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.Property(u => u.Role)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(u => u.FullName)
            .HasMaxLength(100);

        builder.Property(u => u.ProfilePicture)
            .HasMaxLength(255);

        builder.Property(u => u.Location)
            .HasMaxLength(255);

        builder.Property(u => u.Website)
            .HasMaxLength(255);

        builder.Property(u => u.Gender)
            .HasMaxLength(10);
    }
}
