using GoingMy.Shared.Events;
using GoingMy.User.Domain.Entities;
using GoingMy.User.Domain.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Text.Json;

namespace GoingMy.User.Infrastructure.Data;

public class UserDbContext(DbContextOptions<UserDbContext> options) : DbContext(options)
{
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<UserFollow> UserFollows => Set<UserFollow>();
    public DbSet<UserBlock> UserBlocks => Set<UserBlock>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

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
            entity.HasIndex(e => e.Location);
            entity.HasIndex(e => e.IsVerified);
            entity.Property(e => e.Interests)
                .HasColumnType("jsonb")
                .HasDefaultValueSql("'[]'::jsonb");
        });

        modelBuilder.Entity<UserFollow>(entity =>
        {
            entity.ToTable("UserFollows");
            entity.HasKey(e => new { e.FollowerId, e.FolloweeId });
            entity.HasIndex(e => e.FolloweeId);
            entity.HasIndex(e => e.FollowerId);
        });

        modelBuilder.Entity<UserBlock>(entity =>
        {
            entity.ToTable("UserBlocks");
            entity.HasKey(e => new { e.BlockerId, e.BlockeeId });
            entity.HasIndex(e => e.BlockerId);
            entity.HasIndex(e => e.BlockeeId);
        });

        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.ToTable("OutboxMessages");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EventType).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Payload).IsRequired();
            entity.HasIndex(e => e.PublishedAt); // for efficient polling of pending messages
        });
    }
}

/// <summary>
/// EF Core interceptor that automatically creates outbox entries whenever
/// a <see cref="UserProfile"/> is added, modified, or deleted.
/// Messages are committed in the same transaction as the domain write,
/// guaranteeing atomicity without a distributed transaction.
/// </summary>
public class UserProfileOutboxInterceptor : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is UserDbContext context)
        {
            var outboxMessages = CollectOutboxMessages(context);
            if (outboxMessages.Count > 0)
                context.OutboxMessages.AddRange(outboxMessages);
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static List<OutboxMessage> CollectOutboxMessages(UserDbContext context)
    {
        var messages = new List<OutboxMessage>();

        foreach (var entry in context.ChangeTracker.Entries<UserProfile>())
        {
            OutboxMessage? message = entry.State switch
            {
                EntityState.Added => CreateMessage("UserCreatedEvent", new UserCreatedEvent
                {
                    UserId = entry.Entity.Id,
                    Username = entry.Entity.Username,
                    FirstName = entry.Entity.FirstName,
                    LastName = entry.Entity.LastName,
                    AvatarUrl = entry.Entity.AvatarUrl,
                    IsVerified = entry.Entity.IsVerified,
                    CreatedAt = entry.Entity.CreatedAt
                }),
                EntityState.Modified => CreateMessage("UserUpdatedEvent", new UserUpdatedEvent
                {
                    UserId = entry.Entity.Id,
                    Username = entry.Entity.Username,
                    FirstName = entry.Entity.FirstName,
                    LastName = entry.Entity.LastName,
                    AvatarUrl = entry.Entity.AvatarUrl,
                    IsVerified = entry.Entity.IsVerified,
                    UpdatedAt = entry.Entity.UpdatedAt ?? DateTime.UtcNow
                }),
                EntityState.Deleted => CreateMessage("UserDeletedEvent", new UserDeletedEvent
                {
                    UserId = entry.Entity.Id,
                    Username = entry.Entity.Username,
                    DeletedAt = DateTime.UtcNow
                }),
                _ => null
            };

            if (message is not null)
                messages.Add(message);
        }

        return messages;
    }

    private static OutboxMessage CreateMessage<T>(string eventType, T payload) => new()
    {
        EventType = eventType,
        Payload = JsonSerializer.Serialize(payload),
        CreatedAt = DateTime.UtcNow
    };
}

