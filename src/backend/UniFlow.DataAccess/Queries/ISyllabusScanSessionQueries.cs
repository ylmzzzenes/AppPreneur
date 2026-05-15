using UniFlow.Entity.Entities;

namespace UniFlow.DataAccess.Queries;

public interface ISyllabusScanSessionQueries
{
    Task<SyllabusScanSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<SyllabusScanSession?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);

    void Add(SyllabusScanSession session);
}
