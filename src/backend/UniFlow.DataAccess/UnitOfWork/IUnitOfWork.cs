using UniFlow.DataAccess.Repositories;
using UniFlow.Entity.Common;

namespace UniFlow.DataAccess.UnitOfWork;

public interface IUnitOfWork
{
    IRepository<TEntity> Repository<TEntity>()
        where TEntity : BaseEntity;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
