# MasLazu.AspNet.Authentication.Password

The main implementation package for password-based authentication in ASP.NET applications. This package provides the concrete implementation of the password authentication system, including services, validators, utilities, and configuration.

## Overview

This package implements the `IUserPasswordLoginService` interface defined in the Abstraction layer, providing a complete, production-ready password authentication system with:

- Secure password hashing using BCrypt
- Comprehensive input validation
- Database seeding for login methods
- Configurable security policies
- Background services for initialization

## Installation

```bash
dotnet add package MasLazu.AspNet.Authentication.Password
```

## Quick Setup

### Program.cs Configuration

```csharp
builder.Services.AddAuthenticationPasswordApplication(builder.Configuration);
```

This single line adds all necessary components:

- Password authentication service
- Input validators
- Configuration validation
- Database seeding
- Utility services

## Architecture Components

### Service Implementation

#### UserPasswordLoginService

The main service implementing `IUserPasswordLoginService`:

```csharp
public class UserPasswordLoginService : CrudService<UserPasswordLogin, UserPasswordLoginDto, CreateUserPasswordLoginRequest, UpdateUserPasswordLoginRequest>, IUserPasswordLoginService
```

**Key Features:**

- Inherits from `CrudService` for standard CRUD operations
- Implements password-specific authentication methods
- Integrates with user management and verification services
- Handles secure password operations

### Authentication Flow

#### Login Process

```csharp
public async Task<PasswordLoginResponse> LoginAsync(PasswordLoginRequest request, CancellationToken ct)
{
    // 1. Find user by identifier (username/email)
    UserDto user = await _userService.GetByUsernameOrEmailAsync(request.Identifier, ct) ??
        throw new UnauthorizedException("Invalid username/email or password.");

    // 2. Retrieve password login record
    UserPasswordLogin? userPasswordLogin = await ReadRepository.FirstOrDefaultAsync(upl => upl.UserId == user.Id, ct);

    // 3. Verify password hash
    if (userPasswordLogin == null || !PasswordHasher.VerifyPassword(userPasswordLogin.PasswordHash, request.Password))
    {
        throw new UnauthorizedException("Invalid username/email or password.");
    }

    // 4. Check verification status
    if (_passwordConfig.RequireVerification && !userPasswordLogin.IsVerified)
    {
        throw new UnauthorizedException("Account not verified. Please verify your account before logging in.");
    }

    // 5. Generate authentication tokens
    return (await _authService.LoginAsync(userPasswordLogin.UserLoginMethodId, ct)).Adapt<PasswordLoginResponse>();
}
```

#### Registration Process

```csharp
public async Task RegisterAsync(PasswordRegisterRequest request, CancellationToken ct)
{
    // 1. Validate uniqueness
    if (await _userService.IsEmailTakenAsync(request.Email, ct))
        throw new BadRequestException("Email is already taken.");

    if (await _userService.IsUsernameTakenAsync(request.Username, ct))
        throw new BadRequestException("Username is already taken.");

    // 2. Create user account
    var createUserRequest = new CreateUserRequest(/*...*/);
    UserDto userDto = await _userService.CreateAsync(Guid.Empty, createUserRequest, ct);

    // 3. Create login method
    var createLoginMethodRequest = new CreateUserLoginMethodRequest(/*...*/);
    UserLoginMethodDto userLoginMethodDto = await _userLoginMethodService.CreateAsync(Guid.Empty, createLoginMethodRequest, ct);

    // 4. Create password login with hashed password
    var userPasswordLogin = new UserPasswordLogin
    {
        UserId = userDto.Id,
        UserLoginMethodId = userLoginMethodDto.Id,
        PasswordHash = PasswordHasher.HashPassword(request.Password),
        IsVerified = false
    };

    await Repository.AddAsync(userPasswordLogin, ct);
    await UnitOfWork.SaveChangesAsync(ct);

    // 5. Send verification email
    await _userService.SendEmailVerificationAsync(userDto.Email!, ct);
}
```

#### Password Change Process

```csharp
public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct)
{
    // 1. Find user's password record
    UserPasswordLogin userPasswordLogin = await ReadRepository.FirstOrDefaultAsync(upl => upl.UserId == userId, ct) ??
        throw new NotFoundException(nameof(UserPasswordLogin), $"No password login found for user with ID {userId}");

    // 2. Verify current password
    if (!PasswordHasher.VerifyPassword(userPasswordLogin.PasswordHash, request.CurrentPassword))
        throw new UnauthorizedException("Current password is incorrect.");

    // 3. Update with new hashed password
    userPasswordLogin.PasswordHash = PasswordHasher.HashPassword(request.NewPassword);
    await Repository.UpdateAsync(userPasswordLogin, ct);
    await UnitOfWork.SaveChangesAsync(ct);
}
```

## Security Implementation

### Password Hashing

Uses BCrypt with configurable work factor:

```csharp
public static class PasswordHasher
{
    private const int WorkFactor = 12; // 2^12 = 4096 iterations

    public static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    public static bool VerifyPassword(string hashedPassword, string providedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(providedPassword, hashedPassword);
    }
}
```

**Security Features:**

- **BCrypt Algorithm**: Adaptive hashing with salt
- **Work Factor 12**: 4096 iterations for strong security
- **Automatic Salt**: Unique salt per password
- **Rehash Support**: Upgrades old hashes when needed

### Input Validation

#### Password Registration Validation

```csharp
public class PasswordRegisterRequestValidator : AbstractValidator<PasswordRegisterRequest>
{
    public PasswordRegisterRequestValidator(IOptions<PasswordLoginMethodConfiguration> passwordConfig)
    {
        var config = passwordConfig.Value.PasswordValidation;

        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Username).NotEmpty().MaximumLength(50).Matches("^[a-zA-Z0-9_]+$");
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(255);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(config.MinLength);

        if (config.RequireUppercase)
            RuleFor(x => x.Password).Matches("[A-Z]");
        if (config.RequireLowercase)
            RuleFor(x => x.Password).Matches("[a-z]");
        if (config.RequireDigit)
            RuleFor(x => x.Password).Matches("[0-9]");
        if (config.RequireSpecialCharacter)
            RuleFor(x => x.Password).Matches("[^a-zA-Z0-9]");
    }
}
```

#### Login Validation

```csharp
public class PasswordLoginRequestValidator : AbstractValidator<PasswordLoginRequest>
{
    public PasswordLoginRequestValidator()
    {
        RuleFor(x => x.Identifier).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Password).NotEmpty();
    }
}
```

## Configuration

### appsettings.json

```json
{
  "PasswordLoginMethod": {
    "RequireVerification": true,
    "PasswordValidation": {
      "MinLength": 8,
      "RequireUppercase": true,
      "RequireLowercase": true,
      "RequireDigit": true,
      "RequireSpecialCharacter": false
    }
  }
}
```

### Configuration Classes

```csharp
public class PasswordLoginMethodConfiguration
{
    public bool RequireVerification { get; set; } = true;
    public PasswordValidationConfiguration PasswordValidation { get; set; } = new();
}

public class PasswordValidationConfiguration
{
    public int MinLength { get; set; } = 8;
    public bool RequireUppercase { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireDigit { get; set; } = true;
    public bool RequireSpecialCharacter { get; set; } = false;
}
```

## Database Seeding

### Background Service

```csharp
public class AuthenticationPasswordDatabaseSeedBackgroundService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using IServiceScope scope = _serviceScopeFactory.CreateScope();
        ILoginMethodService loginMethodService = scope.ServiceProvider.GetRequiredService<ILoginMethodService>();

        var passwordLoginMethodId = Guid.Parse("019950d6-5fb1-74ad-a33e-2e620dc842c0");
        var createRequest = new CreateLoginMethodRequest("password");
        await loginMethodService.CreateIfNotExistsAsync(passwordLoginMethodId, createRequest, stoppingToken);
    }
}
```

**Purpose:**

- Seeds the password login method in the database
- Runs automatically when the application starts
- Ensures the login method exists before authentication operations

## Extension Methods

### Main Extension

```csharp
public static IServiceCollection AddAuthenticationPasswordApplication(
    this IServiceCollection services,
    IConfiguration configuration)
{
    services.AddAuthenticationPasswordApplicationConfiguration(configuration);
    services.AddAuthenticationPasswordApplicationServices();
    services.AddAuthenticationPasswordApplicationUtils();
    services.AddAuthenticationPasswordApplicationValidators();
    services.AddDatabaseSeedBackgroundService();

    return services;
}
```

### Individual Extensions

- **Configuration**: Registers and validates configuration options
- **Services**: Registers the main authentication service
- **Utils**: Registers utility classes like property mappers
- **Validators**: Registers all FluentValidation validators
- **Background Services**: Registers database seeding services

## Integration Example

### Complete Setup

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add password authentication
builder.Services.AddAuthenticationPasswordApplication(builder.Configuration);

// Add other services (EF Core, etc.)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Use authentication
app.UseAuthentication();
app.UseAuthorization();

app.Run();
```

### Controller Usage

```csharp
[ApiController]
[Route("api/auth")]
public class AuthenticationController : ControllerBase
{
    private readonly IUserPasswordLoginService _authService;

    public AuthenticationController(IUserPasswordLoginService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(PasswordRegisterRequest request)
    {
        await _authService.RegisterAsync(request, CancellationToken.None);
        return Ok("Registration successful. Please check your email for verification.");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(PasswordLoginRequest request)
    {
        try
        {
            var response = await _authService.LoginAsync(request, CancellationToken.None);
            return Ok(response);
        }
        catch (UnauthorizedException)
        {
            return Unauthorized("Invalid credentials");
        }
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        await _authService.ChangePasswordAsync(userId, request, CancellationToken.None);
        return Ok("Password changed successfully");
    }
}
```

## Dependencies

### Package References

- **MasLazu.AspNet.Authentication.Core.Abstraction**: Core authentication interfaces
- **MasLazu.AspNet.Framework.Application**: Base application framework
- **MasLazu.AspNet.Verification.Abstraction**: Email/SMS verification
- **BCrypt.Net-Next**: Password hashing
- **FluentValidation**: Input validation

### Project References

- **MasLazu.AspNet.Authentication.Password.Abstraction**: Interface definitions
- **MasLazu.AspNet.Authentication.Password.Domain**: Domain entities and business logic

## Security Best Practices

1. **Password Hashing**: Always use BCrypt with appropriate work factor
2. **Input Validation**: Validate all inputs using FluentValidation
3. **Account Verification**: Require email verification for new accounts
4. **Secure Configuration**: Use strong password requirements
5. **Error Handling**: Don't leak sensitive information in error messages
6. **Audit Logging**: Track authentication attempts
7. **Rate Limiting**: Implement rate limiting on authentication endpoints

## Error Handling

The service throws specific exceptions for different scenarios:

- **UnauthorizedException**: Invalid credentials or unverified account
- **BadRequestException**: Invalid input data or constraint violations
- **NotFoundException**: User or password record not found
- **ValidationException**: Configuration validation failures

## Testing

### Unit Testing Example

```csharp
public class UserPasswordLoginServiceTests
{
    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsTokenResponse()
    {
        // Arrange
        var mockUserService = new Mock<IUserService>();
        var mockAuthService = new Mock<IAuthService>();
        var service = new UserPasswordLoginService(/* dependencies */);

        // Act
        var result = await service.LoginAsync(loginRequest, CancellationToken.None);

        // Assert
        Assert.NotNull(result.AccessToken);
        Assert.NotNull(result.RefreshToken);
    }
}
```

## Performance Considerations

1. **Password Hashing**: BCrypt is computationally expensive - consider caching for high-traffic scenarios
2. **Database Queries**: Use proper indexing on UserId and UserLoginMethodId
3. **Connection Pooling**: Ensure proper database connection management
4. **Caching**: Consider caching user data to reduce database load
5. **Async Operations**: All operations are async to prevent blocking

## Troubleshooting

### Common Issues

1. **Configuration Not Found**: Ensure `PasswordLoginMethod` section exists in appsettings.json
2. **Database Seeding Fails**: Check database permissions and connection string
3. **Validation Errors**: Verify password requirements match configuration
4. **Hash Verification Fails**: Check if password was hashed with different work factor

### Debug Logging

Enable detailed logging to troubleshoot authentication issues:

```json
{
  "Logging": {
    "LogLevel": {
      "MasLazu.AspNet.Authentication.Password": "Debug"
    }
  }
}
```

This implementation package provides a complete, secure, and production-ready password authentication system that integrates seamlessly with ASP.NET applications while maintaining clean architecture principles.</content>
<parameter name="filePath">/home/mfaziz/projects/cs/MasLazu.AspNet.Authentication.Password/src/MasLazu.AspNet.Authentication.Password/README.md
