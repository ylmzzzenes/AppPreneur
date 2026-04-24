using Microsoft.EntityFrameworkCore;
using UniFlow.DataAccess.Persistence;
using UniFlow.Entity.Entities;

namespace UniFlow.DataAccess.Queries;

public sealed class UserQueries : IUserQueries
{
    private readonly UniFlowDbContext _dbContext;

    public UserQueries(UniFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == normalized, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return await _dbContext.Users
            .AsNoTracking()
            .AnyAsync(u => u.Email == normalized, cancellationToken);
    }
}
