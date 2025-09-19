using MasLazu.AspNet.Framework.Application.Models;

namespace MasLazu.AspNet.Authentication.Password.Abstraction.Models;

public record UpdateUserPasswordLoginRequest(
    Guid Id,
    Guid? UserLoginMethodId,
    string? PasswordHash,
    DateTime? LastLoginDate
) : BaseUpdateRequest(Id);
