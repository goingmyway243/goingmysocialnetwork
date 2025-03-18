using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Social.UserService.Domain.Entities;
using Social.UserService.Domain.Interfaces;
using Social.UserService.Infrastructure.Persistence;

namespace Social.UserService.Infrastructure.Repositories
{
    public class UserRepository : IRepository<UserEntity>
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserEntity?> GetByIdAsync(Guid id)
        {
            return await _context.Users.FirstOrDefaultAsync(user => user.Id == id);
        }

        public IQueryable<UserEntity> GetAll()
        {
            return _context.Users.AsQueryable();
        }

        public async Task<IEnumerable<UserEntity>> GetAllAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<IEnumerable<UserEntity>> FindAsync(Expression<Func<UserEntity, bool>> predicate)
        {
            return await _context.Users.Where(predicate).ToListAsync();
        }

        public async Task InsertAsync(UserEntity entity)
        {
            await _context.Users.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(UserEntity entity)
        {
            _context.Users.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(UserEntity entity)
        {
            _context.Users.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}