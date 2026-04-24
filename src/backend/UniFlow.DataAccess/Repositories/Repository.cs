using Microsoft.EntityFrameworkCore;
using UniFlow.DataAccess.Persistence;
using UniFlow.Entity.Common;

namespace UniFlow.DataAccess.Repositories;

internal sealed class Repository<TEntity> : IRepository<TEntity>
    where TEntity : BaseEntity
{
    private readonly UniFlowDbContext _dbContext;

    public Repository(UniFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TEntity?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<TEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<TEntity?> GetByIdForUpdateAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<TEntity>()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<TEntity>> GetAllReadOnlyAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<TEntity>()
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public void Add(TEntity entity) => _dbContext.Add(entity);

    public void Update(TEntity entity) => _dbContext.Update(entity);

    public void Remove(TEntity entity) => _dbContext.Remove(entity);
}
