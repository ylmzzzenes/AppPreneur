using Microsoft.EntityFrameworkCore;
using UniFlow.DataAccess.Persistence;
using UniFlow.Entity.Entities;

namespace UniFlow.DataAccess.Queries;

public sealed class SyllabusScanSessionQueries(UniFlowDbContext dbContext) : ISyllabusScanSessionQueries
{
    public Task<SyllabusScanSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.SyllabusScanSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public Task<SyllabusScanSession?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.SyllabusScanSessions
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public void Add(SyllabusScanSession session) => dbContext.SyllabusScanSessions.Add(session);
}
