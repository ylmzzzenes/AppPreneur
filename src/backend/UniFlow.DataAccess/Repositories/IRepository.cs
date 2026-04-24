using UniFlow.Entity.Common;

namespace UniFlow.DataAccess.Repositories;

public interface IRepository<TEntity>
    where TEntity : BaseEntity
{
    Task<TEntity?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tracked load for updates in the same unit-of-work.
    /// </summary>
    Task<TEntity?> GetByIdForUpdateAsync(long id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TEntity>> GetAllReadOnlyAsync(CancellationToken cancellationToken = default);

    void Add(TEntity entity);

    void Update(TEntity entity);

    void Remove(TEntity entity);
}
