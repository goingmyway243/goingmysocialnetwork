using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Social.UserService.Domain.Entities;
using Social.UserService.Domain.Interfaces;
using Social.UserService.Infrastructure.Persistence;

namespace Social.UserService.Infrastructure.Repositories
{
    public class FriendshipRepository : IRepository<FriendshipEntity>
    {
        private readonly ApplicationDbContext _context;

        public FriendshipRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<FriendshipEntity?> GetByIdAsync(Guid id)
        {
            return await _context.Friendships.FirstOrDefaultAsync(f => f.Id == id);
        }

        public IQueryable<FriendshipEntity> GetAll()
        {
            return _context.Friendships.AsQueryable();
        }

        public async Task<IEnumerable<FriendshipEntity>> GetAllAsync()
        {
            return await _context.Friendships.ToListAsync();
        }

        public async Task<IEnumerable<FriendshipEntity>> FindAsync(Expression<Func<FriendshipEntity, bool>> predicate)
        {
            return await _context.Friendships.Where(predicate).ToListAsync();
        }

        public async Task InsertAsync(FriendshipEntity entity)
        {
            await _context.Friendships.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(FriendshipEntity entity)
        {
            _context.Friendships.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(FriendshipEntity entity)
        {
            _context.Friendships.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}