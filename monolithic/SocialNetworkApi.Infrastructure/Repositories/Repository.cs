using System.Linq.Expressions;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SocialNetworkApi.Domain.Common;
using SocialNetworkApi.Domain.Interfaces;
using SocialNetworkApi.Infrastructure.Persistence;

namespace SocialNetworkApi.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _dbSet = context.Set<T>();
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate);
    }

    public IQueryable<T> GetAll()
    {
        return _dbSet.AsQueryable();
    }

    public async Task<List<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    public async Task InsertAsync(T entity)
    {
        if (entity is AuditedEntity auditedEntity)
        {
            auditedEntity.CreatedBy = GetCurrentUserId();
        }

        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(T entity)
    {
        if (entity is AuditedEntity auditedEntity)
        {
            auditedEntity.ModifiedBy = GetCurrentUserId();
            auditedEntity.ModifiedAt = DateTime.UtcNow;
        }

        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
    }

    private Guid GetCurrentUserId()
    {
        var userId = _httpContextAccessor?.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(userId, out var result))
        {
            return result;
        }

        return default;
    }
}
