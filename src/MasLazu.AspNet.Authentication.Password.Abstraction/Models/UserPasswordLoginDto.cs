using MasLazu.AspNet.Framework.Application.Models;

namespace MasLazu.AspNet.Authentication.Password.Abstraction.Models;

public record UserPasswordLoginDto(
    Guid Id,
    Guid UserLoginMethodId,
    string PasswordHash,
    DateTime? LastLoginDate,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
) : BaseDto(Id, CreatedAt, UpdatedAt);
