using System.Linq.Expressions;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using SocialNetworkApi.Domain.Common;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMongoCollection<T> _collection;

    public Repository(IMongoDatabase database, IHttpContextAccessor httpContextAccessor, string collectionName)
    {
        _collection = database.GetCollection<T>(collectionName);
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await _collection.Find(p => p.Id == id).FirstOrDefaultAsync();
    }

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        return await _collection.Find(predicate).FirstOrDefaultAsync();
    }

    public IMongoCollection<T> GetAll()
    {
        return _collection;
    }

    public async Task<List<T>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _collection.Find(predicate).ToListAsync();
    }

    public async Task InsertAsync(T entity)
    {
        if (entity is AuditedEntity auditedEntity)
        {
            auditedEntity.CreatedBy = GetCurrentUserId();
        }

        await _collection.InsertOneAsync(entity);
    }

    public async Task UpdateAsync(T entity)
    {
        if (entity is AuditedEntity auditedEntity)
        {
            auditedEntity.ModifiedBy = GetCurrentUserId();
            auditedEntity.ModifiedAt = DateTime.UtcNow;
        }

        await _collection.ReplaceOneAsync(p => p.Id == entity.Id, entity);
    }

    public async Task DeleteAsync(T entity)
    {
        await _collection.DeleteOneAsync(p => p.Id == entity.Id);
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
