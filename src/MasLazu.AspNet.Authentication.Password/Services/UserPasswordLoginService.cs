using FluentValidation;
using Mapster;
using Microsoft.Extensions.Options;
using MasLazu.AspNet.Authentication.Core.Abstraction.Interfaces;
using MasLazu.AspNet.Authentication.Core.Abstraction.Models;
using MasLazu.AspNet.Authentication.Password.Abstraction.Interfaces;
using MasLazu.AspNet.Authentication.Password.Abstraction.Models;
using MasLazu.AspNet.Verification.Abstraction.Interfaces;
using MasLazu.AspNet.Verification.Abstraction.Models;
using MasLazu.AspNet.Authentication.Password.Configurations;
using MasLazu.AspNet.Authentication.Password.Constants;
using MasLazu.AspNet.Authentication.Password.Utils;
using MasLazu.AspNet.Authentication.Password.Domain.Entities;
using MasLazu.AspNet.Authentication.Password.Exceptions;
using MasLazu.AspNet.Framework.Application.Exceptions;
using MasLazu.AspNet.Framework.Application.Interfaces;
using MasLazu.AspNet.Framework.Application.Services;

namespace MasLazu.AspNet.Authentication.Password.Services;

public class UserPasswordLoginService : CrudService<UserPasswordLogin, UserPasswordLoginDto, CreateUserPasswordLoginRequest, UpdateUserPasswordLoginRequest>, IUserPasswordLoginService
{
    private readonly IUserService _userService;
    private readonly IAuthService _authService;
    private readonly IUserLoginMethodService _userLoginMethodService;
    private readonly PasswordLoginMethodConfiguration _passwordConfig;
    private readonly IValidator<PasswordRegisterRequest> _registerValidator;

    public UserPasswordLoginService(
        IRepository<UserPasswordLogin> repository,
        IReadRepository<UserPasswordLogin> readRepository,
        IUnitOfWork unitOfWork,
        IEntityPropertyMap<UserPasswordLogin> propertyMap,
        IPaginationValidator<UserPasswordLogin> paginationValidator,
        ICursorPaginationValidator<UserPasswordLogin> cursorPaginationValidator,
        IUserService userService,
        IAuthService authService,
        IUserLoginMethodService userLoginMethodService,
        IOptions<PasswordLoginMethodConfiguration> passwordConfig,
        IValidator<PasswordRegisterRequest> registerValidator,
        IValidator<CreateUserPasswordLoginRequest>? createValidator = null,
        IValidator<UpdateUserPasswordLoginRequest>? updateValidator = null)
        : base(repository, readRepository, unitOfWork, propertyMap, paginationValidator, cursorPaginationValidator, createValidator, updateValidator)
    {
        _userService = userService;
        _authService = authService;
        _userLoginMethodService = userLoginMethodService;
        _passwordConfig = passwordConfig.Value;
        _registerValidator = registerValidator;
    }

    public async Task<PasswordLoginResponse> LoginAsync(PasswordLoginRequest request, CancellationToken ct)
    {
        UserDto user = await _userService.GetByUsernameOrEmailAsync(request.Identifier, ct) ??
            throw new InvalidCredentialsException(request.Identifier);

        UserPasswordLogin? userPasswordLogin = await ReadRepository.FirstOrDefaultAsync(upl => upl.UserId == user.Id, ct);

        if (userPasswordLogin == null || !PasswordHasher.VerifyPassword(userPasswordLogin.PasswordHash, request.Password))
        {
            throw new InvalidCredentialsException(request.Identifier);
        }

        if (_passwordConfig.RequireVerification && !userPasswordLogin.IsVerified)
        {
            throw new AccountNotVerifiedException(user.Email);
        }

        return (await _authService.LoginAsync(userPasswordLogin.UserLoginMethodId, ct)).Adapt<PasswordLoginResponse>();
    }

    public async Task RegisterAsync(PasswordRegisterRequest request, CancellationToken ct)
    {
        await ValidateAsync(request, _registerValidator, ct);

        if (await _userService.IsEmailTakenAsync(request.Email, ct))
        {
            throw new EmailAlreadyTakenException(request.Email);
        }

        if (await _userService.IsUsernameTakenAsync(request.Username, ct))
        {
            throw new UsernameAlreadyTakenException(request.Username);
        }

        var createUserRequest = new CreateUserRequest(
            Name: request.Name,
            Email: request.Email,
            PhoneNumber: null,
            Username: request.Username,
            LanguageCode: null,
            TimezoneId: null,
            GenderCode: null
        );

        UserDto userDto = await _userService.CreateAsync(Guid.Empty, createUserRequest, false, ct);

        var createLoginMethodRequest = new CreateUserLoginMethodRequest(
            UserId: userDto.Id,
            LoginMethodCode: PasswordConstants.LoginMethodCode
        );

        UserLoginMethodDto userLoginMethodDto = await _userLoginMethodService.CreateAsync(Guid.Empty, createLoginMethodRequest, false, ct);

        var userPasswordLogin = new UserPasswordLogin
        {
            UserId = userDto.Id,
            UserLoginMethodId = userLoginMethodDto.Id,
            PasswordHash = PasswordHasher.HashPassword(request.Password),
            IsVerified = false
        };

        await Repository.AddAsync(userPasswordLogin, ct);
        await UnitOfWork.SaveChangesAsync(ct);
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct)
    {
        UserPasswordLogin userPasswordLogin = await ReadRepository.FirstOrDefaultAsync(upl => upl.UserId == userId, ct) ??
            throw new PasswordLoginNotFoundException(userId);

        if (!PasswordHasher.VerifyPassword(userPasswordLogin.PasswordHash, request.CurrentPassword))
        {
            throw new InvalidCurrentPasswordException();
        }

        userPasswordLogin.PasswordHash = PasswordHasher.HashPassword(request.NewPassword);
        await Repository.UpdateAsync(userPasswordLogin, ct);
        await UnitOfWork.SaveChangesAsync(ct);
    }
}