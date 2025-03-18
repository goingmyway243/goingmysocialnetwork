using System.Reflection;
using Microsoft.EntityFrameworkCore;
using SocialNetworkApi.Domain.Entities;

namespace SocialNetworkApi.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<UserEntity> Users { get; set; }
    public DbSet<FriendshipEntity> Friendships { get; set; }
    public DbSet<PostEntity> Posts { get; set; }
    public DbSet<ContentEntity> Contents { get; set; }
    public DbSet<LikeEntity> Likes { get; set; }
    public DbSet<CommentEntity> Comments { get; set; }
    public DbSet<ChatroomEntity> ChatRooms { get; set; }
    public DbSet<ChatMessageEntity> ChatMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}