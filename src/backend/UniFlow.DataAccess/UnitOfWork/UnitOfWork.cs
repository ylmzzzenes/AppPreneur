using UniFlow.DataAccess.Persistence;
using UniFlow.DataAccess.Repositories;
using UniFlow.Entity.Common;

namespace UniFlow.DataAccess.UnitOfWork;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly UniFlowDbContext _dbContext;
    private readonly Dictionary<Type, object> _repositories = new();

    public UnitOfWork(UniFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IRepository<TEntity> Repository<TEntity>()
        where TEntity : BaseEntity
    {
        var type = typeof(TEntity);
        if (_repositories.TryGetValue(type, out var existing))
        {
            return (IRepository<TEntity>)existing;
        }

        var repository = new Repository<TEntity>(_dbContext);
        _repositories[type] = repository;
        return repository;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}
