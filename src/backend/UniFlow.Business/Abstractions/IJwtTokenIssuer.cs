using UniFlow.Entity.Entities;

namespace UniFlow.Business.Abstractions;

public interface IJwtTokenIssuer
{
    (string Token, DateTime ExpiresAtUtc) CreateAccessToken(User user);
}
