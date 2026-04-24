using UniFlow.Entity.Entities;

namespace UniFlow.DataAccess.Queries;

public interface IUserQueries
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
}
