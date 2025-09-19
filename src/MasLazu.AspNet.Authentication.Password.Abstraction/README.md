# MasLazu.AspNet.Authentication.Password.Abstraction

A clean, secure, and extensible abstraction layer for password-based authentication in ASP.NET applications.

## Overview

This package provides the core abstractions and data models for implementing password-based authentication in ASP.NET applications. It defines interfaces, request/response models, and DTOs that form the foundation of a robust authentication system.

## Features

- 🔐 **Secure Password Handling**: Passwords are never stored in plain text
- 🏗️ **Clean Architecture**: Separation of concerns with clear abstraction layers
- 🔄 **Async/Await Support**: All operations support asynchronous programming with cancellation tokens
- 🎯 **Flexible Authentication**: Supports multiple authentication methods per user
- 📊 **Activity Tracking**: Tracks login activity for security monitoring
- 🛡️ **Type Safety**: Uses C# records and nullable reference types for compile-time safety
- 🔗 **Framework Integration**: Built on top of MasLazu.AspNet.Framework.Application

## Installation

```bash
dotnet add package MasLazu.AspNet.Authentication.Password.Abstraction
```

## Core Interface

### IUserPasswordLoginService

The main interface that defines the contract for password authentication operations:

```csharp
public interface IUserPasswordLoginService : ICrudService<UserPasswordLoginDto, CreateUserPasswordLoginRequest, UpdateUserPasswordLoginRequest>
{
    Task<PasswordLoginResponse> LoginAsync(PasswordLoginRequest request, CancellationToken ct);
    Task RegisterAsync(PasswordRegisterRequest request, CancellationToken ct);
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct);
}
```

## Data Models

### Authentication Models

#### PasswordLoginRequest

Request model for user login:

```csharp
public record PasswordLoginRequest(
    string Identifier,  // Email or username
    string Password
);
```

#### PasswordRegisterRequest

Request model for user registration:

```csharp
public record PasswordRegisterRequest(
    string Name,
    string Username,
    string Email,
    string Password
);
```

#### ChangePasswordRequest

Request model for password changes:

```csharp
public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword
);
```

#### PasswordLoginResponse

Response model containing authentication tokens:

```csharp
public record PasswordLoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiresAt,
    DateTimeOffset RefreshTokenExpiresAt,
    string TokenType = "Bearer"
);
```

### Data Transfer Objects

#### UserPasswordLoginDto

Main DTO representing a user's password login information:

```csharp
public record UserPasswordLoginDto(
    Guid Id,
    Guid UserLoginMethodId,
    string PasswordHash,
    DateTime? LastLoginDate,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
) : BaseDto(Id, CreatedAt, UpdatedAt);
```

#### CRUD Operation Models

**CreateUserPasswordLoginRequest:**

```csharp
public record CreateUserPasswordLoginRequest(
    Guid UserLoginMethodId,
    string PasswordHash  // Already hashed
);
```

**UpdateUserPasswordLoginRequest:**

```csharp
public record UpdateUserPasswordLoginRequest(
    Guid Id,
    Guid? UserLoginMethodId,
    string? PasswordHash,
    DateTime? LastLoginDate
) : BaseUpdateRequest(Id);
```

## Quick Start

### Basic Usage

```csharp
// Register the service
services.AddScoped<IUserPasswordLoginService, YourPasswordLoginService>();

// Use in controller
[HttpPost("login")]
public async Task<IActionResult> Login(PasswordLoginRequest request)
{
    var response = await _authService.LoginAsync(request, CancellationToken.None);
    return Ok(response);
}
```

## Usage Example

### Implementing the Service

```csharp
public class UserPasswordLoginService : IUserPasswordLoginService
{
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _jwtGenerator;

    public async Task<PasswordLoginResponse> LoginAsync(PasswordLoginRequest request, CancellationToken ct)
    {
        // Find user by identifier (email/username)
        var user = await _userRepository.FindByIdentifierAsync(request.Identifier, ct);
        if (user == null)
            throw new UnauthorizedAccessException("Invalid credentials");

        // Verify password
        var isValidPassword = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash);
        if (!isValidPassword)
            throw new UnauthorizedAccessException("Invalid credentials");

        // Generate tokens
        var tokens = await _jwtGenerator.GenerateTokensAsync(user, ct);

        // Update last login date
        await UpdateLastLoginDateAsync(user.Id, ct);

        return new PasswordLoginResponse(
            tokens.AccessToken,
            tokens.RefreshToken,
            tokens.AccessTokenExpiresAt,
            tokens.RefreshTokenExpiresAt
        );
    }

    // Implement other interface methods...
}
```

### Registering in DI Container

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddScoped<IUserPasswordLoginService, UserPasswordLoginService>();
    services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
    services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
}
```

### Using in Controller

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

    [HttpPost("login")]
    public async Task<IActionResult> Login(PasswordLoginRequest request)
    {
        try
        {
            var response = await _authService.LoginAsync(request, CancellationToken.None);
            return Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized("Invalid credentials");
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(PasswordRegisterRequest request)
    {
        await _authService.RegisterAsync(request, CancellationToken.None);
        return Ok("User registered successfully");
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        await _authService.ChangePasswordAsync(userId, request, CancellationToken.None);
        return Ok("Password changed successfully");
    }
}
```

## Security Best Practices

1. **Password Hashing**: Always hash passwords before storage using strong algorithms like bcrypt or Argon2
2. **Salt Usage**: Use unique salts for each password
3. **Token Security**: Implement proper JWT token validation and refresh token rotation
4. **Rate Limiting**: Implement rate limiting on authentication endpoints
5. **Audit Logging**: Log authentication attempts for security monitoring
6. **Password Policies**: Enforce strong password requirements
7. **Account Lockout**: Implement temporary account lockout after failed attempts

## Dependencies

- **Target Framework**: .NET 9.0
- **MasLazu.AspNet.Framework.Application**: ^1.0.0-preview.6

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Ensure all tests pass
6. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Related Packages

- **MasLazu.AspNet.Authentication.Password**: Main implementation package
- **MasLazu.AspNet.Authentication.Password.Domain**: Domain entities and business logic
- **MasLazu.AspNet.Authentication.Password.EfCore**: Entity Framework Core implementation
- **MasLazu.AspNet.Authentication.Password.Endpoint**: Minimal API endpoints</content>
  <parameter name="filePath">/home/mfaziz/projects/cs/MasLazu.AspNet.Authentication.Password/src/MasLazu.AspNet.Authentication.Password.Abstraction/README.md
