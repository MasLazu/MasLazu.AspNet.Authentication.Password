# MasLazu.AspNet.Authentication.Password.Endpoint

Minimal API endpoints for password-based authentication using FastEndpoints. This package provides RESTful HTTP endpoints for user registration, login, and password management with comprehensive error handling and validation.

## Overview

This package implements the **API layer** of the password authentication system using FastEndpoints, providing:

- **RESTful Endpoints**: Clean HTTP API for authentication operations
- **Request Validation**: Automatic model validation with detailed error responses
- **Error Handling**: Consistent error responses with appropriate HTTP status codes
- **Security**: Proper authentication and authorization handling
- **Documentation**: OpenAPI/Swagger integration with endpoint metadata

## Installation

```bash
dotnet add package MasLazu.AspNet.Authentication.Password.Endpoint
```

## API Endpoints

### Authentication Endpoint Group

All authentication endpoints are grouped under `/api/v1/auth`:

```csharp
public class AuthEndpointGroup : SubGroup<V1EndpointGroup>
{
    public AuthEndpointGroup()
    {
        Configure("auth", ep => ep.Description(x => x.WithTags("Auth")));
    }
}
```

**Base URL**: `POST /api/v1/auth/{endpoint}`

### 1. User Registration

**Endpoint**: `POST /api/v1/auth/register`

**Request Body**:

```json
{
  "name": "John Doe",
  "username": "johndoe",
  "email": "john.doe@example.com",
  "password": "SecurePass123!"
}
```

**Response**: `200 OK`

```json
{
  "success": true,
  "message": "Register Successful",
  "data": null
}
```

**Implementation**:

```csharp
public class RegisterEndpoint : BaseEndpointWithoutResponse<PasswordRegisterRequest>
{
    public IUserPasswordLoginService UserPasswordLoginService { get; set; }

    public override void ConfigureEndpoint()
    {
        Post("/register");
        Group<AuthEndpointGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(PasswordRegisterRequest req, CancellationToken ct)
    {
        await UserPasswordLoginService.RegisterAsync(req, ct);
        await SendOkResponseAsync("Register Successful", ct);
    }
}
```

### 2. User Login

**Endpoint**: `POST /api/v1/auth/login`

**Request Body**:

```json
{
  "identifier": "john.doe@example.com",
  "password": "SecurePass123!"
}
```

**Response**: `200 OK`

```json
{
  "success": true,
  "message": "Login Successful",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "refresh_token_here",
    "accessTokenExpiresAt": "2024-12-20T10:30:00Z",
    "refreshTokenExpiresAt": "2025-12-20T10:30:00Z",
    "tokenType": "Bearer"
  }
}
```

**Implementation**:

```csharp
public class LoginEndpoint : BaseEndpoint<PasswordLoginRequest, PasswordLoginResponse>
{
    public IUserPasswordLoginService UserPasswordLoginService { get; set; }

    public override void ConfigureEndpoint()
    {
        Post("/login");
        Group<AuthEndpointGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(PasswordLoginRequest req, CancellationToken ct)
    {
        PasswordLoginResponse response = await UserPasswordLoginService.LoginAsync(req, ct);
        await SendOkResponseAsync(response, "Login Successful", ct);
    }
}
```

### 3. Change Password

**Endpoint**: `POST /api/v1/auth/change-password`

**Headers**:

```
Authorization: Bearer {access_token}
```

**Request Body**:

```json
{
  "currentPassword": "CurrentPass123!",
  "newPassword": "NewSecurePass456!"
}
```

**Response**: `200 OK`

```json
{
  "success": true,
  "message": "Change Password Successful",
  "data": null
}
```

**Implementation**:

```csharp
public class ChangePasswordEndpoint : BaseEndpointWithoutResponse<ChangePasswordRequest>
{
    public IUserPasswordLoginService UserPasswordLoginService { get; set; }

    public override void ConfigureEndpoint()
    {
        Post("/change-password");
        Group<AuthEndpointGroup>();
        // Requires authentication by default
    }

    public override async Task HandleAsync(ChangePasswordRequest req, CancellationToken ct)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
        {
            throw new UnauthorizedException("User is not authenticated");
        }

        await UserPasswordLoginService.ChangePasswordAsync(userId, req, ct);
        await SendOkResponseAsync("Change Password Successful", ct);
    }
}
```

## FastEndpoints Architecture

### Endpoint Base Classes

The endpoints inherit from framework base classes that provide:

#### BaseEndpoint<TRequest, TResponse>

- For endpoints that accept a request and return a response
- Automatic model binding and validation
- Structured response formatting

#### BaseEndpointWithoutResponse<TRequest>

- For endpoints that accept a request but don't return data
- Used for operations like registration and password changes
- Still provides success/failure responses

### Key Features

#### Automatic Validation

```csharp
// Request validation happens automatically
public override void ConfigureEndpoint()
{
    Post("/register");
    Group<AuthEndpointGroup>();
    AllowAnonymous();
}
// Validation errors return 400 Bad Request with details
```

#### Dependency Injection

```csharp
// Services are injected automatically
public IUserPasswordLoginService UserPasswordLoginService { get; set; }
```

#### Response Formatting

```csharp
// Consistent response structure
await SendOkResponseAsync(response, "Login Successful", ct);
// Returns: { success: true, message: "Login Successful", data: response }
```

## Security Implementation

### Authentication & Authorization

#### Anonymous Endpoints

```csharp
AllowAnonymous(); // Login and Register endpoints
```

#### Authenticated Endpoints

```csharp
// ChangePassword requires authentication
Post("/change-password");
Group<AuthEndpointGroup>();
// Authentication is required by default
```

#### User Context

```csharp
// Extract user ID from JWT token
var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
{
    throw new UnauthorizedException("User is not authenticated");
}
```

### Error Handling

#### Structured Error Responses

**Validation Errors**: `400 Bad Request`

```json
{
  "success": false,
  "message": "One or more validation errors occurred.",
  "errors": {
    "Email": ["The Email field is required."],
    "Password": ["Password must be at least 8 characters long."]
  }
}
```

**Authentication Errors**: `401 Unauthorized`

```json
{
  "success": false,
  "message": "Invalid username/email or password.",
  "data": null
}
```

**Business Logic Errors**: `400 Bad Request`

```json
{
  "success": false,
  "message": "Email is already taken.",
  "data": null
}
```

## Setup and Configuration

### Program.cs Integration

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add authentication services
builder.Services.AddAuthenticationPasswordApplication(builder.Configuration);

// Add endpoints
// Endpoints are automatically discovered by FastEndpoints

var app = builder.Build();

// Configure middleware
app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.UseFastEndpoints();

app.Run();
```

### FastEndpoints Configuration

```csharp
// In Program.cs
builder.Services.AddFastEndpoints();

// Configure Swagger/OpenAPI
builder.Services.AddSwaggerDoc();
```

## API Documentation

### OpenAPI/Swagger Integration

The endpoints are automatically documented with:

- **Request/Response Schemas**: Generated from C# models
- **Validation Rules**: Documented constraints and requirements
- **Authentication Requirements**: Security scheme documentation
- **Example Requests/Responses**: Sample data for testing

### Swagger UI

Access the API documentation at: `https://your-api/swagger`

## Request/Response Models

### PasswordRegisterRequest

```csharp
public record PasswordRegisterRequest(
    string Name,
    string Username,
    string Email,
    string Password
);
```

### PasswordLoginRequest

```csharp
public record PasswordLoginRequest(
    string Identifier,  // Email or username
    string Password
);
```

### ChangePasswordRequest

```csharp
public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword
);
```

### PasswordLoginResponse

```csharp
public record PasswordLoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiresAt,
    DateTimeOffset RefreshTokenExpiresAt,
    string TokenType = "Bearer"
);
```

## Error Scenarios

### Common Error Cases

1. **Invalid Credentials**

   ```json
   {
     "success": false,
     "message": "Invalid username/email or password.",
     "data": null
   }
   ```

2. **Account Not Verified**

   ```json
   {
     "success": false,
     "message": "Account not verified. Please verify your account before logging in.",
     "data": null
   }
   ```

3. **Duplicate Email/Username**

   ```json
   {
     "success": false,
     "message": "Email is already taken.",
     "data": null
   }
   ```

4. **Validation Failures**
   ```json
   {
     "success": false,
     "message": "One or more validation errors occurred.",
     "errors": {
       "Password": ["Password must contain at least one uppercase letter."]
     }
   }
   ```

## Testing

### Endpoint Testing

```csharp
public class AuthenticationEndpointsTests : TestBase
{
    [Fact]
    public async Task Register_WithValidData_ReturnsSuccess()
    {
        var request = new PasswordRegisterRequest(
            Name: "Test User",
            Username: "testuser",
            Email: "test@example.com",
            Password: "TestPass123!"
        );

        var (response, result) = await Client.POSTAsync<RegisterEndpoint, PasswordRegisterRequest>(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Register Successful");
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        var request = new PasswordLoginRequest(
            Identifier: "invalid@example.com",
            Password: "wrongpassword"
        );

        var (response, result) = await Client.POSTAsync<LoginEndpoint, PasswordLoginRequest, ErrorResponse>(request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        result.Success.Should().BeFalse();
    }
}
```

### Integration Testing

```csharp
[Fact]
public async Task FullAuthenticationFlow_WorksCorrectly()
{
    // 1. Register user
    var registerRequest = new PasswordRegisterRequest(/*...*/);
    await Client.POSTAsync<RegisterEndpoint, PasswordRegisterRequest>(registerRequest);

    // 2. Login
    var loginRequest = new PasswordLoginRequest(/*...*/);
    var (loginResponse, loginResult) = await Client.POSTAsync<LoginEndpoint, PasswordLoginRequest, ApiResponse<PasswordLoginResponse>>(loginRequest);

    // 3. Use token for authenticated requests
    Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.Data.AccessToken);

    // 4. Change password
    var changeRequest = new ChangePasswordRequest(/*...*/);
    await Client.POSTAsync<ChangePasswordEndpoint, ChangePasswordRequest>(changeRequest);
}
```

## Performance Considerations

### Endpoint Optimization

1. **Async Operations**: All endpoints use async/await
2. **Dependency Injection**: Services are injected once per request
3. **Model Validation**: Client-side validation prevents unnecessary server calls
4. **Response Compression**: Enable gzip compression for API responses

### Rate Limiting

```csharp
// Configure rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("auth", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 10;
    });
});

// Apply to endpoints
public override void ConfigureEndpoint()
{
    Post("/login");
    Group<AuthEndpointGroup>();
    AllowAnonymous();
    Throttle("auth"); // Apply rate limiting
}
```

## Monitoring and Logging

### Request Logging

```csharp
// Automatic request/response logging via FastEndpoints
app.UseFastEndpoints(c =>
{
    c.Endpoints.Configurator = ep =>
    {
        ep.PreProcessor<RequestLogger>(Order.Before);
        ep.PostProcessor<ResponseLogger>(Order.After);
    };
});
```

### Health Checks

```csharp
// Add authentication health check
builder.Services.AddHealthChecks()
    .AddCheck<AuthenticationHealthCheck>("authentication");
```

## Dependencies

### Package References

- **FastEndpoints**: ^7.0.1 (Minimal API framework)
- **MasLazu.AspNet.Framework.Endpoint**: ^1.0.0-preview.6 (Base endpoint classes)

### Project References

- **MasLazu.AspNet.Authentication.Password.Abstraction**: Interface and model definitions

## API Versioning

The endpoints follow REST API versioning:

- **Current Version**: v1
- **Base Path**: `/api/v1/auth`
- **Version Strategy**: URL path versioning

Future versions can be added by creating new endpoint groups:

```csharp
public class AuthV2EndpointGroup : SubGroup<V2EndpointGroup>
{
    public AuthV2EndpointGroup()
    {
        Configure("auth", ep => ep.Description(x => x.WithTags("Auth V2")));
    }
}
```

## Best Practices

1. **Consistent Response Format**: All endpoints return structured responses
2. **Proper HTTP Status Codes**: 200 for success, 400 for validation, 401 for auth
3. **Input Validation**: Comprehensive validation with clear error messages
4. **Security Headers**: CORS, HSTS, and other security headers
5. **Documentation**: Complete OpenAPI documentation
6. **Testing**: Comprehensive unit and integration tests
7. **Monitoring**: Request logging and health checks
8. **Performance**: Async operations and efficient data access

This endpoint package provides a complete, secure, and well-documented REST API for password authentication with FastEndpoints, following modern API design principles and best practices.</content>
<parameter name="filePath">/home/mfaziz/projects/cs/MasLazu.AspNet.Authentication.Password/src/MasLazu.AspNet.Authentication.Password.Endpoint/README.md
