using System.Linq.Expressions;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SocialNetworkApi.Domain.Common;
using SocialNetworkApi.Domain.Interfaces;
using SocialNetworkApi.Infrastructure.Persistence;

namespace SocialNetworkApi.Infrastructure.Repositories;

public class TransientRepository<T> : ITransientRepository<T> where T : class
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TransientRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory, IHttpContextAccessor httpContextAccessor)
    {
        _dbContextFactory = dbContextFactory;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var dbSet = context.Set<T>();
        
        return await dbSet.FindAsync(id);
    }

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var dbSet = context.Set<T>();
        
        return await dbSet.FirstOrDefaultAsync(predicate);
    }

    public IQueryable<T> GetAll()
    {
        using var context = _dbContextFactory.CreateDbContext();
        var dbSet = context.Set<T>();
        
        return dbSet.AsQueryable();
    }

    public async Task<List<T>> GetAllAsync()
    {
        using var context = _dbContextFactory.CreateDbContext();
        var dbSet = context.Set<T>();
        
        return await dbSet.ToListAsync();
    }

    public async Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var dbSet = context.Set<T>();
        
        return await dbSet.Where(predicate).ToListAsync();
    }

    public async Task InsertAsync(T entity)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var dbSet = context.Set<T>();
        
        if (entity is AuditedEntity auditedEntity)
        {
            auditedEntity.CreatedBy = GetCurrentUserId();
        }

        await dbSet.AddAsync(entity);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(T entity)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var dbSet = context.Set<T>();
        
        if (entity is AuditedEntity auditedEntity)
        {
            auditedEntity.ModifiedBy = GetCurrentUserId();
            auditedEntity.ModifiedAt = DateTime.UtcNow;
        }

        dbSet.Update(entity);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(T entity)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var dbSet = context.Set<T>();
        
        dbSet.Remove(entity);
        await context.SaveChangesAsync();
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
