using Microsoft.EntityFrameworkCore;
using Social.UserService.Domain.Entities;

namespace Social.UserService.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            Database.EnsureCreated();
            Database.Migrate();
        }

        public DbSet<UserEntity> Users { get; set; }
        public DbSet<FriendshipEntity> Friendships { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}