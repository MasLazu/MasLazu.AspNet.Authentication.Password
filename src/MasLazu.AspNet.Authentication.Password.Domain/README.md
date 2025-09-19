# MasLazu.AspNet.Authentication.Password.Domain

The domain layer containing the core business entities and business logic for password-based authentication in ASP.NET applications.

## Overview

This package defines the domain model for password authentication, including the core entity `UserPasswordLogin` and the business rules that govern password authentication behavior.

## Core Entity

### UserPasswordLogin

The central domain entity representing a user's password-based login credentials:

```csharp
public class UserPasswordLogin : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid UserLoginMethodId { get; set; }
    public bool IsVerified { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime? LastLoginDate { get; set; }
}
```

**Entity Properties:**

- **UserId**: References the user account
- **UserLoginMethodId**: Links to the login method configuration
- **IsVerified**: Indicates if the account has been verified (email/SMS verification)
- **PasswordHash**: BCrypt-hashed password (never stores plain text)
- **LastLoginDate**: Tracks the last successful login for security monitoring

## Business Logic Implementation

### Password Authentication Service

The domain logic is implemented in `UserPasswordLoginService` which handles:

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
    // 1. Validate email uniqueness
    if (await _userService.IsEmailTakenAsync(request.Email, ct))
    {
        throw new BadRequestException("Email is already taken.");
    }

    // 2. Validate username uniqueness
    if (await _userService.IsUsernameTakenAsync(request.Username, ct))
    {
        throw new BadRequestException("Username is already taken.");
    }

    // 3. Create user account
    var createUserRequest = new CreateUserRequest(/*...*/);
    UserDto userDto = await _userService.CreateAsync(Guid.Empty, createUserRequest, ct);

    // 4. Create login method
    var createLoginMethodRequest = new CreateUserLoginMethodRequest(/*...*/);
    UserLoginMethodDto userLoginMethodDto = await _userLoginMethodService.CreateAsync(Guid.Empty, createLoginMethodRequest, ct);

    // 5. Create password login record
    var userPasswordLogin = new UserPasswordLogin
    {
        UserId = userDto.Id,
        UserLoginMethodId = userLoginMethodDto.Id,
        PasswordHash = PasswordHasher.HashPassword(request.Password),
        IsVerified = false
    };

    await Repository.AddAsync(userPasswordLogin, ct);
    await UnitOfWork.SaveChangesAsync(ct);

    // 6. Send verification email
    await _userService.SendEmailVerificationAsync(userDto.Email!, ct);
}
```

#### Password Change Process

```csharp
public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct)
{
    // 1. Find user's password login record
    UserPasswordLogin userPasswordLogin = await ReadRepository.FirstOrDefaultAsync(upl => upl.UserId == userId, ct) ??
        throw new NotFoundException(nameof(UserPasswordLogin), $"No password login found for user with ID {userId}");

    // 2. Verify current password
    if (!PasswordHasher.VerifyPassword(userPasswordLogin.PasswordHash, request.CurrentPassword))
    {
        throw new UnauthorizedException("Current password is incorrect.");
    }

    // 3. Hash and update new password
    userPasswordLogin.PasswordHash = PasswordHasher.HashPassword(request.NewPassword);
    await Repository.UpdateAsync(userPasswordLogin, ct);
    await UnitOfWork.SaveChangesAsync(ct);
}
```

## Security Features

### Password Hashing

The domain uses BCrypt for secure password hashing:

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

    public static bool NeedsRehash(string hashedPassword)
    {
        return BCrypt.Net.BCrypt.PasswordNeedsRehash(hashedPassword, WorkFactor);
    }

    public static string RehashIfNeeded(string hashedPassword, string plainPassword)
    {
        if (NeedsRehash(hashedPassword))
        {
            return HashPassword(plainPassword);
        }
        return hashedPassword;
    }
}
```

**Security Features:**

- **BCrypt Algorithm**: Industry-standard adaptive hashing
- **Work Factor 12**: 4096 iterations for strong security
- **Salt Generation**: Automatic salt generation per password
- **Rehashing Support**: Automatic upgrade of old hashes when needed

### Account Verification

```csharp
public class PasswordLoginMethodConfiguration
{
    public bool RequireVerification { get; set; } = true;
    public PasswordValidationConfiguration PasswordValidation { get; set; } = new();
}
```

- **Email Verification**: New accounts require email verification before login
- **Configurable**: Can be disabled for development/testing
- **Security Enforcement**: Prevents unauthorized access to unverified accounts

### Password Validation Configuration

```csharp
public class PasswordValidationConfiguration
{
    public int MinLength { get; set; } = 8;
    public bool RequireUppercase { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireDigit { get; set; } = true;
    public bool RequireSpecialCharacter { get; set; } = false;
}
```

## Data Integrity

### Entity Configuration

```csharp
public class UserPasswordLoginConfiguration : IEntityTypeConfiguration<UserPasswordLogin>
{
    public void Configure(EntityTypeBuilder<UserPasswordLogin> builder)
    {
        builder.HasKey(upl => upl.Id);

        builder.Property(upl => upl.UserLoginMethodId)
            .IsRequired();

        builder.Property(upl => upl.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasIndex(upl => upl.UserLoginMethodId)
            .IsUnique();
    }
}
```

**Database Constraints:**

- **Primary Key**: Unique identifier for each password login record
- **Required Fields**: UserLoginMethodId and PasswordHash are mandatory
- **Unique Index**: One password login per user login method
- **Length Limits**: Password hash limited to 500 characters

### Property Mapping

```csharp
public class UserPasswordLoginEntityPropertyMap : IEntityPropertyMap<Domain.Entities.UserPasswordLogin>
{
    private readonly Dictionary<string, Expression<Func<Domain.Entities.UserPasswordLogin, object>>> _map =
        new(StringComparer.OrdinalIgnoreCase)
        {
            { "id", upl => upl.Id },
            { "userLoginMethodId", upl => upl.UserLoginMethodId },
            { "passwordHash", upl => upl.PasswordHash },
            { "lastLoginDate", upl => upl.LastLoginDate! },
            { "createdAt", upl => upl.CreatedAt },
            { "updatedAt", upl => upl.UpdatedAt! }
        };
}
```

## Business Rules

### Authentication Rules

1. **User Existence**: User must exist in the system
2. **Password Verification**: Password must match stored hash
3. **Account Verification**: Account must be verified (if required)
4. **Login Tracking**: Last login date is updated on successful authentication

### Registration Rules

1. **Email Uniqueness**: Email must not be already registered
2. **Username Uniqueness**: Username must not be already taken
3. **Password Hashing**: Plain text passwords are immediately hashed
4. **Verification Required**: New accounts start as unverified

### Password Change Rules

1. **Current Password**: Must provide correct current password
2. **User Ownership**: Can only change password for own account
3. **Immediate Update**: Password changes take effect immediately

## Dependencies

- **Target Framework**: .NET 9.0
- **MasLazu.AspNet.Framework.Domain**: ^1.0.0-preview.6
- **BCrypt.Net-Next**: For password hashing
- **FluentValidation**: For request validation
- **Mapster**: For object mapping

## Architecture Role

This domain layer serves as the **business logic core** in the hexagonal architecture:

```
┌─────────────────────────────────────┐
│         INFRASTRUCTURE              │
│   (EF Core, External Services)      │
└─────────────────────────────────────┘
                    │
           ┌────────┴────────┐
           │   APPLICATION   │
           │   (Services)    │
           └─────────────────┘
                    │
           ┌────────┴────────┐
           │     DOMAIN      │
           │  (Business      │
           │   Logic)        │
           │                 │
           │ • Password      │
           │   Validation    │
           │ • User Auth     │
           │ • Security      │
           │   Policies      │
           └─────────────────┘
```

The domain layer is **independent** of infrastructure concerns and contains:

- **Entities**: Core business objects
- **Business Rules**: Authentication logic and validation
- **Security Policies**: Password requirements and verification rules
- **Data Integrity**: Entity constraints and relationships

## Security Considerations

1. **Never Store Plain Text**: Passwords are always hashed before storage
2. **Strong Hashing**: BCrypt with work factor 12 provides robust security
3. **Account Verification**: Prevents unauthorized access to new accounts
4. **Login Tracking**: Monitors authentication activity for security
5. **Unique Constraints**: Prevents duplicate registrations
6. **Input Validation**: All inputs are validated before processing

This domain layer provides a solid foundation for secure, scalable password authentication while maintaining clean separation of concerns and testability.</content>
<parameter name="filePath">/home/mfaziz/projects/cs/MasLazu.AspNet.Authentication.Password/src/MasLazu.AspNet.Authentication.Password.Domain/README.md
