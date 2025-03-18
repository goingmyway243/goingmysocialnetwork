using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialNetworkApi.Domain.Entities;

namespace SocialNetworkApi.Infrastructure.Persistence.Configurations;

public class ContentEntityConfiguration : IEntityTypeConfiguration<ContentEntity>
{
    public void Configure(EntityTypeBuilder<ContentEntity> builder)
    {
        builder.Property(c => c.LinkContent)
            .HasMaxLength(255);

        builder.Property(c => c.Type)
            .HasConversion<string>()
            .HasMaxLength(50);
    }
}
