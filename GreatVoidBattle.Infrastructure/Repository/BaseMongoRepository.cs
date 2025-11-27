using MongoDB.Driver;
using System.Linq.Expressions;

namespace GreatVoidBattle.Infrastructure.Repository;

/// <summary>
/// Base MongoDB repository with common CRUD operations.
/// Reduces code duplication across repositories.
/// </summary>
/// <typeparam name="TEntity">Entity type</typeparam>
/// <typeparam name="TId">Entity identifier type</typeparam>
public abstract class BaseMongoRepository<TEntity, TId> where TEntity : class
{
    protected readonly IMongoCollection<TEntity> _collection;

    protected BaseMongoRepository(IMongoDatabase database, string collectionName)
    {
        _collection = database.GetCollection<TEntity>(collectionName);
    }

    /// <summary>
    /// Gets the filter expression for finding entity by ID.
    /// Must be implemented by derived classes.
    /// </summary>
    protected abstract Expression<Func<TEntity, bool>> GetByIdFilter(TId id);

    /// <summary>
    /// Called before updating an entity. Override to update timestamps etc.
    /// </summary>
    protected virtual void OnBeforeUpdate(TEntity entity)
    {
        // Override in derived classes to set UpdatedAt timestamp etc.
    }

    public virtual async Task<TEntity?> GetByIdAsync(TId id)
    {
        return await _collection.Find(GetByIdFilter(id)).FirstOrDefaultAsync();
    }

    public virtual async Task<List<TEntity>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public virtual async Task<TEntity> CreateAsync(TEntity entity)
    {
        await _collection.InsertOneAsync(entity);
        return entity;
    }

    public virtual async Task UpdateAsync(TEntity entity, TId id)
    {
        OnBeforeUpdate(entity);
        await _collection.ReplaceOneAsync(GetByIdFilter(id), entity);
    }

    public virtual async Task DeleteAsync(TId id)
    {
        await _collection.DeleteOneAsync(GetByIdFilter(id));
    }

    /// <summary>
    /// Finds entities matching the filter
    /// </summary>
    protected async Task<List<TEntity>> FindAsync(Expression<Func<TEntity, bool>> filter)
    {
        return await _collection.Find(filter).ToListAsync();
    }

    /// <summary>
    /// Finds first entity matching the filter
    /// </summary>
    protected async Task<TEntity?> FindFirstAsync(Expression<Func<TEntity, bool>> filter)
    {
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }
}

