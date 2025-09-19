# MasLazu.AspNet.Authentication.Password

A comprehensive, secure, and extensible password-based authentication system for ASP.NET applications built with Clean Architecture and modern .NET practices.

## Overview

This solution provides a complete authentication framework that implements password-based user authentication with enterprise-grade security, scalability, and maintainability. Built using Clean Architecture principles, it offers a modular design that separates concerns while providing a seamless developer experience.

## 🏗️ Architecture

The solution follows **Clean Architecture** (Hexagonal Architecture) with clear separation of concerns:

```
┌─────────────────────────────────────────────────────────────┐
│                    PRESENTATION LAYER                       │
│  ┌─────────────────────────────────────────────────────────┐ │
│  │              MasLazu.AspNet.Authentication.Password.Endpoint │
│  │              (REST API Endpoints - FastEndpoints)       │ │
│  └─────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
                                │
                ┌───────────────┴───────────────┐
                │        APPLICATION LAYER      │
                │  ┌────────────────────────────┴────────────────────────────┐ │
                │  │   MasLazu.AspNet.Authentication.Password               │ │
                │  │   (Business Logic, Services, Validation)               │ │
                │  └─────────────────────────────────────────────────────────┘ │
                └─────────────────────────────────────────────────────────────┘
                                                │
                                ┌───────────────┴───────────────┐
                                │        DOMAIN LAYER          │
                                │  ┌────────────────────────────┴────────────────────────────┐ │
                                │  │   MasLazu.AspNet.Authentication.Password.Domain        │ │
                                │  │   (Entities, Business Rules, Security)                 │ │
                                │  └─────────────────────────────────────────────────────────┘ │
                                └─────────────────────────────────────────────────────────────┘
                                                │
                                ┌───────────────┴───────────────┐
                                │     INFRASTRUCTURE LAYER     │
                                │  ┌────────────────────────────┴────────────────────────────┐ │
                                │  │   MasLazu.AspNet.Authentication.Password.EfCore        │ │
                                │  │   (Database Access, CQRS, Migrations)                  │ │
                                │  └─────────────────────────────────────────────────────────┘ │
                                └─────────────────────────────────────────────────────────────┘
                                                │
                                ┌───────────────┴───────────────┐
                                │        SHARED LAYER          │
                                │  ┌────────────────────────────┴────────────────────────────┐ │
                                │  │   MasLazu.AspNet.Authentication.Password.Abstraction   │ │
                                │  │   (Contracts, DTOs, Interfaces)                        │ │
                                │  └─────────────────────────────────────────────────────────┘ │
                                └─────────────────────────────────────────────────────────────┘
```

## 📦 Solution Structure

### Core Packages

| Package                                                | Purpose                                 | Key Features                                                 |
| ------------------------------------------------------ | --------------------------------------- | ------------------------------------------------------------ |
| **MasLazu.AspNet.Authentication.Password.Abstraction** | Interface definitions and contracts     | DTOs, request/response models, service interfaces            |
| **MasLazu.AspNet.Authentication.Password.Domain**      | Domain entities and business logic      | UserPasswordLogin entity, password hashing, validation rules |
| **MasLazu.AspNet.Authentication.Password**             | Application services and business logic | Service implementations, validation, configuration           |
| **MasLazu.AspNet.Authentication.Password.EfCore**      | Data access layer                       | Entity Framework Core, CQRS, migrations                      |
| **MasLazu.AspNet.Authentication.Password.Endpoint**    | REST API endpoints                      | FastEndpoints, OpenAPI, request/response handling            |

## 🚀 Quick Start

### 1. Installation

```bash
# Add the main package
dotnet add package MasLazu.AspNet.Authentication.Password

# Add EF Core support
dotnet add package MasLazu.AspNet.Authentication.Password.EfCore

# Add API endpoints
dotnet add package MasLazu.AspNet.Authentication.Password.Endpoint
```

### 2. Configuration

**appsettings.json:**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=AuthDb;Trusted_Connection=True;"
  },
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

**Program.cs:**

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add authentication services
builder.Services.AddAuthenticationPasswordApplication(builder.Configuration);

// Add database context
builder.Services.AddDbContext<PasswordDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure middleware
app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.UseFastEndpoints();

app.Run();
```

### 3. Database Setup

```bash
# Create migration
dotnet ef migrations add InitialCreate --project src/MasLazu.AspNet.Authentication.Password.EfCore

# Apply migration
dotnet ef database update
```

## 🔐 Authentication Features

### User Registration

- **Secure Password Hashing**: BCrypt with configurable work factor
- **Input Validation**: Comprehensive validation with detailed error messages
- **Duplicate Prevention**: Email and username uniqueness enforcement
- **Email Verification**: Optional account verification workflow

### User Login

- **Flexible Authentication**: Support for email or username login
- **Secure Verification**: BCrypt password verification
- **Account Status**: Verification requirement checking
- **Token Generation**: JWT access and refresh token creation

### Password Management

- **Secure Changes**: Current password verification required
- **Hash Updates**: Automatic rehashing with new passwords
- **Audit Trail**: Login activity tracking

### Security Features

- **BCrypt Hashing**: Industry-standard password hashing
- **Configurable Policies**: Password complexity requirements
- **Account Verification**: Email verification workflow
- **Rate Limiting**: Protection against brute force attacks
- **Audit Logging**: Authentication activity monitoring

## 📋 API Endpoints

### Authentication Endpoints

| Method | Endpoint                       | Description       | Auth Required |
| ------ | ------------------------------ | ----------------- | ------------- |
| `POST` | `/api/v1/auth/register`        | User registration | ❌            |
| `POST` | `/api/v1/auth/login`           | User login        | ❌            |
| `POST` | `/api/v1/auth/change-password` | Password change   | ✅            |

### Example Usage

#### Register User

```bash
curl -X POST https://api.example.com/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "name": "John Doe",
    "username": "johndoe",
    "email": "john.doe@example.com",
    "password": "SecurePass123!"
  }'
```

#### Login User

```bash
curl -X POST https://api.example.com/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "identifier": "john.doe@example.com",
    "password": "SecurePass123!"
  }'
```

#### Change Password

```bash
curl -X POST https://api.example.com/api/v1/auth/change-password \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {access_token}" \
  -d '{
    "currentPassword": "SecurePass123!",
    "newPassword": "NewSecurePass456!"
  }'
```

## 🛠️ Advanced Configuration

### Password Policies

```json
{
  "PasswordLoginMethod": {
    "RequireVerification": true,
    "PasswordValidation": {
      "MinLength": 12,
      "RequireUppercase": true,
      "RequireLowercase": true,
      "RequireDigit": true,
      "RequireSpecialCharacter": true
    }
  }
}
```

### Database Configuration

```csharp
// CQRS Setup
builder.Services.AddDbContext<PasswordDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDbContext<PasswordReadDbContext>(options =>
    options.UseSqlServer(connectionString));

// Repository Registration
builder.Services.AddScoped<IRepository<UserPasswordLogin>, EfRepository<UserPasswordLogin, PasswordDbContext>>();
builder.Services.AddScoped<IReadRepository<UserPasswordLogin>, EfReadRepository<UserPasswordLogin, PasswordReadDbContext>>();
```

### Custom Validation

```csharp
// Add custom validators
builder.Services.AddScoped<IValidator<CustomRegistrationRequest>, CustomRegistrationValidator>();
```

## 🧪 Testing

### Unit Testing

```csharp
public class PasswordAuthenticationTests
{
    [Fact]
    public async Task Register_WithValidData_CreatesUser()
    {
        // Arrange
        var service = new UserPasswordLoginService(/* dependencies */);

        // Act
        await service.RegisterAsync(registerRequest, CancellationToken.None);

        // Assert
        // Verify user creation and email sending
    }
}
```

### Integration Testing

```csharp
[Collection("Database")]
public class AuthenticationIntegrationTests
{
    [Fact]
    public async Task FullAuthenticationFlow_WorksCorrectly()
    {
        // Register → Verify Email → Login → Change Password
    }
}
```

### Endpoint Testing

```csharp
public class AuthenticationEndpointsTests : TestBase
{
    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokens()
    {
        var request = new PasswordLoginRequest("user@example.com", "password");
        var (response, result) = await Client.POSTAsync<LoginEndpoint, PasswordLoginRequest, ApiResponse<PasswordLoginResponse>>(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Data.AccessToken.Should().NotBeNull();
    }
}
```

## 🔒 Security Best Practices

### Password Security

- ✅ **BCrypt Hashing**: Work factor 12 (4096 iterations)
- ✅ **Salt Generation**: Automatic unique salts
- ✅ **Rehashing**: Automatic upgrade of old hashes
- ✅ **Never Store Plain Text**: Passwords hashed immediately

### Account Security

- ✅ **Email Verification**: Prevent unauthorized access
- ✅ **Rate Limiting**: Protection against brute force
- ✅ **Audit Logging**: Track authentication attempts
- ✅ **Secure Tokens**: JWT with proper expiration

### Application Security

- ✅ **Input Validation**: Comprehensive request validation
- ✅ **SQL Injection Prevention**: Parameterized queries
- ✅ **XSS Protection**: Proper output encoding
- ✅ **CSRF Protection**: Anti-forgery tokens

## 📊 Performance & Scalability

### Database Optimization

- **CQRS Pattern**: Separate read/write databases
- **Strategic Indexing**: Optimized for common queries
- **Connection Pooling**: Efficient database connections
- **Query Optimization**: AsNoTracking for read operations

### Caching Strategies

```csharp
// User data caching
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IUserCache, UserMemoryCache>();
```

### Background Processing

```csharp
// Email verification background service
builder.Services.AddHostedService<EmailVerificationBackgroundService>();
```

## 🔧 Development & Contribution

### Prerequisites

- **.NET 9.0** or later
- **SQL Server** (or compatible database)
- **Visual Studio 2022** or VS Code

### Building the Solution

```bash
# Restore packages
dotnet restore

# Build solution
dotnet build

# Run tests
dotnet test
```

### Code Organization

```
src/
├── MasLazu.AspNet.Authentication.Password.Abstraction/  # Contracts & DTOs
├── MasLazu.AspNet.Authentication.Password.Domain/       # Entities & Business Logic
├── MasLazu.AspNet.Authentication.Password/              # Application Services
├── MasLazu.AspNet.Authentication.Password.EfCore/       # Data Access
└── MasLazu.AspNet.Authentication.Password.Endpoint/     # API Endpoints
```

### Contributing Guidelines

1. **Fork** the repository
2. **Create** a feature branch
3. **Write** comprehensive tests
4. **Ensure** all tests pass
5. **Submit** a pull request

## 📚 Documentation

### Package Documentation

- [**Abstraction Layer**](./src/MasLazu.AspNet.Authentication.Password.Abstraction/README.md) - Interface definitions and contracts
- [**Domain Layer**](./src/MasLazu.AspNet.Authentication.Password.Domain/README.md) - Business entities and rules
- [**Application Layer**](./src/MasLazu.AspNet.Authentication.Password/README.md) - Service implementations
- [**EF Core Layer**](./src/MasLazu.AspNet.Authentication.Password.EfCore/README.md) - Data access and migrations
- [**Endpoint Layer**](./src/MasLazu.AspNet.Authentication.Password.Endpoint/README.md) - REST API documentation

### API Documentation

Access the interactive API documentation at:

```
https://your-api/swagger
```

## 🐛 Troubleshooting

### Common Issues

1. **Migration Errors**

   ```bash
   # Reset migrations
   dotnet ef database drop
   dotnet ef migrations remove
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

2. **Configuration Issues**

   - Verify `appsettings.json` structure
   - Check connection string validity
   - Ensure password policy configuration

3. **Authentication Failures**
   - Verify JWT configuration
   - Check token expiration
   - Validate password hashing settings

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🤝 Support

- **Issues**: [GitHub Issues](https://github.com/maslazu/auth-password/issues)
- **Discussions**: [GitHub Discussions](https://github.com/maslazu/auth-password/discussions)
- **Documentation**: [Wiki](https://github.com/maslazu/auth-password/wiki)

## 🏆 Key Benefits

- ✅ **Production Ready**: Enterprise-grade security and performance
- ✅ **Clean Architecture**: Modular design with clear separation of concerns
- ✅ **Highly Testable**: Comprehensive unit and integration test support
- ✅ **Extensible**: Easy to customize and extend functionality
- ✅ **Well Documented**: Complete API documentation and examples
- ✅ **Modern .NET**: Built with .NET 9.0 and latest frameworks
- ✅ **Security First**: Industry-standard security practices
- ✅ **Performance Optimized**: CQRS, caching, and database optimization

---

**Built with ❤️ using Clean Architecture and modern .NET practices**</content>
<parameter name="filePath">/home/mfaziz/projects/cs/MasLazu.AspNet.Authentication.Password/README.md
