using UniFlow.Entity.Entities;
using UniFlow.Entity.Enums;

namespace UniFlow.DataAccess.Queries;

public interface IUserQueries
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

    Task<PersonalityVibe?> GetPersonalityVibeAsync(long userId, CancellationToken cancellationToken = default);
}
