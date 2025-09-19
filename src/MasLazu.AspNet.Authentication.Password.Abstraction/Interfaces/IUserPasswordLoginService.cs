using MasLazu.AspNet.Framework.Application.Interfaces;
using MasLazu.AspNet.Authentication.Password.Abstraction.Models;

namespace MasLazu.AspNet.Authentication.Password.Abstraction.Interfaces;

public interface IUserPasswordLoginService : ICrudService<UserPasswordLoginDto, CreateUserPasswordLoginRequest, UpdateUserPasswordLoginRequest>
{
    Task<PasswordLoginResponse> LoginAsync(PasswordLoginRequest request, CancellationToken ct);
    Task RegisterAsync(PasswordRegisterRequest request, CancellationToken ct);
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct);
}
