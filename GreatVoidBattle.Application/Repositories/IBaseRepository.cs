namespace GreatVoidBattle.Application.Repositories;

/// <summary>
/// Base repository interface with common CRUD operations.
/// TEntity is the entity type, TId is the type of the entity's identifier.
/// </summary>
public interface IBaseRepository<TEntity, TId> where TEntity : class
{
    Task<TEntity?> GetByIdAsync(TId id);
    Task<List<TEntity>> GetAllAsync();
    Task<TEntity> CreateAsync(TEntity entity);
    Task UpdateAsync(TEntity entity);
    Task DeleteAsync(TId id);
}

