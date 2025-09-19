# MasLazu.AspNet.Authentication.Password.EfCore

Entity Framework Core implementation for password-based authentication persistence layer. This package provides the database access layer with optimized configurations, CQRS support, and clean integration patterns.

## Overview

This package implements the data access layer for password authentication using Entity Framework Core, featuring:

- **CQRS Pattern**: Separate read and write database contexts
- **Optimized Configurations**: Performance-tuned entity mappings
- **Migration Support**: Database schema management
- **Clean Architecture**: Infrastructure layer separation

## Installation

```bash
dotnet add package MasLazu.AspNet.Authentication.Password.EfCore
```

## Architecture

### CQRS Implementation

The package implements Command Query Responsibility Segregation (CQRS) with two separate DbContext classes:

#### Write Context (Commands)

```csharp
public class PasswordDbContext : BaseDbContext
{
    public PasswordDbContext(DbContextOptions<PasswordDbContext> options) : base(options)
    {
    }

    public DbSet<UserPasswordLogin> UserPasswordLogins { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
```

#### Read Context (Queries)

```csharp
public class PasswordReadDbContext : BaseReadDbContext
{
    public PasswordReadDbContext(DbContextOptions<PasswordReadDbContext> options) : base(options)
    {
    }

    public DbSet<UserPasswordLogin> UserPasswordLogins { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
```

**CQRS Benefits:**

- **Performance**: Read operations can use optimized indexes and caching
- **Scalability**: Read and write workloads can scale independently
- **Security**: Read models can exclude sensitive data
- **Maintainability**: Clear separation of concerns

## Entity Configuration

### UserPasswordLogin Configuration

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

**Configuration Details:**

#### Primary Key

```csharp
builder.HasKey(upl => upl.Id);
```

- Uses GUID as primary key (inherited from BaseEntity)
- Ensures unique identification of each password login record

#### Required Properties

```csharp
builder.Property(upl => upl.UserLoginMethodId)
    .IsRequired();
```

- UserLoginMethodId is mandatory
- Links to the authentication method configuration

#### Password Hash Storage

```csharp
builder.Property(upl => upl.PasswordHash)
    .IsRequired()
    .HasMaxLength(500);
```

- Password hash is required (never null)
- Maximum length of 500 characters accommodates BCrypt hashes
- BCrypt hashes are typically ~60 characters but allows buffer for future algorithms

#### Unique Constraint

```csharp
builder.HasIndex(upl => upl.UserLoginMethodId)
    .IsUnique();
```

- Ensures one password login per user login method
- Prevents duplicate password entries for the same authentication method
- Database-level constraint for data integrity

## Database Schema

### Generated Table Structure

```sql
CREATE TABLE [UserPasswordLogins] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [UserLoginMethodId] uniqueidentifier NOT NULL,
    [IsVerified] bit NOT NULL,
    [PasswordHash] nvarchar(500) NOT NULL,
    [LastLoginDate] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_UserPasswordLogins] PRIMARY KEY ([Id]),
    CONSTRAINT [UQ_UserPasswordLogins_UserLoginMethodId] UNIQUE ([UserLoginMethodId])
);

-- Indexes
CREATE INDEX [IX_UserPasswordLogins_UserId] ON [UserPasswordLogins] ([UserId]);
CREATE INDEX [IX_UserPasswordLogins_IsVerified] ON [UserPasswordLogins] ([IsVerified]);
CREATE INDEX [IX_UserPasswordLogins_LastLoginDate] ON [UserPasswordLogins] ([LastLoginDate]);
```

### Indexes and Performance

**Automatically Created Indexes:**

- **Primary Key Index**: On `Id` column
- **Unique Index**: On `UserLoginMethodId` (enforces uniqueness)
- **Foreign Key Indexes**: On `UserId` (if relationships are configured)

**Recommended Additional Indexes:**

```sql
-- For login performance
CREATE INDEX [IX_UserPasswordLogins_UserId_IsVerified] ON [UserPasswordLogins] ([UserId], [IsVerified]);

-- For cleanup operations
CREATE INDEX [IX_UserPasswordLogins_CreatedAt] ON [UserPasswordLogins] ([CreatedAt]);

-- For audit queries
CREATE INDEX [IX_UserPasswordLogins_LastLoginDate] ON [UserPasswordLogins] ([LastLoginDate]) WHERE [LastLoginDate] IS NOT NULL;
```

## Setup and Configuration

### Dependency Injection Setup

```csharp
// Program.cs or Startup.cs
builder.Services.AddDbContext<PasswordDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<PasswordReadDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

### Repository Registration

```csharp
// Register repositories
builder.Services.AddScoped<IRepository<UserPasswordLogin>, EfRepository<UserPasswordLogin, PasswordDbContext>>();
builder.Services.AddScoped<IReadRepository<UserPasswordLogin>, EfReadRepository<UserPasswordLogin, PasswordReadDbContext>>();
builder.Services.AddScoped<IUnitOfWork, EfUnitOfWork<PasswordDbContext>>();
```

### Migration Setup

```bash
# Add migration
dotnet ef migrations add InitialCreate --project src/MasLazu.AspNet.Authentication.Password.EfCore --startup-project src/YourWebProject

# Update database
dotnet ef database update --project src/MasLazu.AspNet.Authentication.Password.EfCore --startup-project src/YourWebProject
```

## Usage Examples

### Basic CRUD Operations

```csharp
public class UserPasswordLoginService : IUserPasswordLoginService
{
    private readonly IRepository<UserPasswordLogin> _repository;
    private readonly IReadRepository<UserPasswordLogin> _readRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UserPasswordLoginService(
        IRepository<UserPasswordLogin> repository,
        IReadRepository<UserPasswordLogin> readRepository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _readRepository = readRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UserPasswordLogin?> GetByUserIdAsync(Guid userId)
    {
        return await _readRepository.FirstOrDefaultAsync(upl => upl.UserId == userId);
    }

    public async Task CreateAsync(UserPasswordLogin userPasswordLogin)
    {
        await _repository.AddAsync(userPasswordLogin);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UpdateAsync(UserPasswordLogin userPasswordLogin)
    {
        await _repository.UpdateAsync(userPasswordLogin);
        await _unitOfWork.SaveChangesAsync();
    }
}
```

### Query Examples

```csharp
// Find by user ID
var passwordLogin = await _readRepository.FirstOrDefaultAsync(upl => upl.UserId == userId);

// Find verified users
var verifiedUsers = await _readRepository.Where(upl => upl.IsVerified).ToListAsync();

// Find users who logged in recently
var recentLogins = await _readRepository
    .Where(upl => upl.LastLoginDate > DateTime.UtcNow.AddDays(-7))
    .OrderByDescending(upl => upl.LastLoginDate)
    .ToListAsync();

// Count total password logins
var totalCount = await _readRepository.CountAsync();

// Paginated results
var page = await _readRepository
    .OrderBy(upl => upl.CreatedAt)
    .Skip((pageNumber - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync();
```

## Performance Optimization

### Connection String Optimization

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=YourDb;Trusted_Connection=True;MultipleActiveResultSets=true;Max Pool Size=100;Min Pool Size=5;Connection Timeout=30;"
  }
}
```

### Query Optimization

```csharp
// Use AsNoTracking for read-only queries
var users = await _readRepository
    .AsNoTracking()
    .Where(upl => upl.IsVerified)
    .ToListAsync();

// Use projections for better performance
var userInfo = await _readRepository
    .Where(upl => upl.UserId == userId)
    .Select(upl => new { upl.Id, upl.LastLoginDate, upl.IsVerified })
    .FirstOrDefaultAsync();
```

### Batch Operations

```csharp
// Bulk update last login dates
var usersToUpdate = await _readRepository
    .Where(upl => upl.UserId == userId)
    .ToListAsync();

foreach (var user in usersToUpdate)
{
    user.LastLoginDate = DateTime.UtcNow;
}

await _repository.UpdateRangeAsync(usersToUpdate);
await _unitOfWork.SaveChangesAsync();
```

## Migration Strategy

### Creating Migrations

```bash
# Create migration for new changes
dotnet ef migrations add AddPasswordExpiration --project src/MasLazu.AspNet.Authentication.Password.EfCore

# Apply migration
dotnet ef database update
```

### Migration Files Structure

```
Migrations/
├── 20231201120000_InitialCreate.cs
├── 20231201120000_InitialCreate.Designer.cs
├── 20231201123000_AddPasswordExpiration.cs
├── 20231201123000_AddPasswordExpiration.Designer.cs
└── PasswordDbContextModelSnapshot.cs
```

### Handling Data Changes

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // Add new column
    migrationBuilder.AddColumn<DateTime>(
        name: "PasswordExpiresAt",
        table: "UserPasswordLogins",
        nullable: true);

    // Create index
    migrationBuilder.CreateIndex(
        name: "IX_UserPasswordLogins_PasswordExpiresAt",
        table: "UserPasswordLogins",
        column: "PasswordExpiresAt");
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DropIndex(
        name: "IX_UserPasswordLogins_PasswordExpiresAt");

    migrationBuilder.DropColumn(
        name: "PasswordExpiresAt",
        table: "UserPasswordLogins");
}
```

## Testing

### Unit Testing with In-Memory Database

```csharp
public class PasswordAuthenticationTests
{
    private PasswordDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<PasswordDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new PasswordDbContext(options);
    }

    [Fact]
    public async Task CreatePasswordLogin_StoresCorrectly()
    {
        using var context = CreateInMemoryContext();
        var repository = new EfRepository<UserPasswordLogin, PasswordDbContext>(context);

        var passwordLogin = new UserPasswordLogin
        {
            UserId = Guid.NewGuid(),
            UserLoginMethodId = Guid.NewGuid(),
            PasswordHash = "hashed_password",
            IsVerified = false
        };

        await repository.AddAsync(passwordLogin);
        await context.SaveChangesAsync();

        var stored = await context.UserPasswordLogins.FindAsync(passwordLogin.Id);
        Assert.NotNull(stored);
        Assert.Equal(passwordLogin.PasswordHash, stored.PasswordHash);
    }
}
```

### Integration Testing

```csharp
[Collection("Database")]
public class PasswordAuthenticationIntegrationTests
{
    private readonly PasswordDbContext _context;

    public PasswordAuthenticationIntegrationTests(DatabaseFixture fixture)
    {
        _context = fixture.Context;
    }

    [Fact]
    public async Task FullAuthenticationFlow_WorksCorrectly()
    {
        // Test complete authentication workflow
        // Register → Verify → Login → Change Password
    }
}
```

## Monitoring and Diagnostics

### Health Checks

```csharp
public static class DatabaseHealthChecks
{
    public static IHealthChecksBuilder AddPasswordDatabaseHealthCheck(
        this IHealthChecksBuilder builder,
        string connectionString)
    {
        builder.AddSqlServer(
            connectionString,
            name: "Password Database",
            tags: new[] { "database", "password-auth" });

        return builder;
    }
}
```

### Logging

```csharp
public class PasswordAuthenticationLoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ILogger<PasswordAuthenticationLoggingBehavior<TRequest, TResponse>> _logger;

    public PasswordAuthenticationLoggingBehavior(ILogger<PasswordAuthenticationLoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling {RequestType}", typeof(TRequest).Name);

        var response = await next();

        _logger.LogInformation("Handled {RequestType} successfully", typeof(TRequest).Name);

        return response;
    }
}
```

## Security Considerations

### Data Protection

1. **Encryption at Rest**: Ensure database encryption is enabled
2. **Connection Security**: Use encrypted connections (SSL/TLS)
3. **Access Control**: Implement proper database permissions
4. **Audit Logging**: Log all authentication attempts

### Query Security

```csharp
// Avoid SQL injection with parameterized queries
var user = await _readRepository.FirstOrDefaultAsync(
    upl => upl.UserId == userId && upl.IsVerified == true);

// Use compiled queries for performance
private static readonly Func<PasswordReadDbContext, Guid, Task<UserPasswordLogin?>> GetByUserIdCompiled =
    EF.CompileAsyncQuery((PasswordReadDbContext context, Guid userId) =>
        context.UserPasswordLogins.FirstOrDefault(upl => upl.UserId == userId));
```

## Troubleshooting

### Common Issues

1. **Migration Errors**: Check connection string and database permissions
2. **Performance Issues**: Add appropriate indexes and optimize queries
3. **Concurrency Conflicts**: Implement proper concurrency handling
4. **Connection Pool Exhaustion**: Monitor and configure connection pool settings

### Debug Queries

```csharp
// Enable sensitive data logging (development only)
builder.Services.AddDbContext<PasswordDbContext>(options =>
    options.UseSqlServer(connectionString)
           .EnableSensitiveDataLogging()
           .EnableDetailedErrors());

// Log queries
options.LogTo(Console.WriteLine, LogLevel.Information)
       .EnableSensitiveDataLogging();
```

## Dependencies

### Package References

- **MasLazu.AspNet.Framework.EfCore**: ^1.0.0-preview.6 (Base EF Core functionality)
- **Microsoft.EntityFrameworkCore**: ^9.0.9 (EF Core runtime)

### Project References

- **MasLazu.AspNet.Authentication.Password.Domain**: Domain entities and business logic

## Best Practices

1. **Use CQRS**: Separate read and write operations for better performance
2. **Index Strategically**: Add indexes for frequently queried columns
3. **Monitor Performance**: Use EF Core logging and profiling tools
4. **Handle Concurrency**: Implement optimistic concurrency control
5. **Test Thoroughly**: Use both unit and integration tests
6. **Secure Configuration**: Never log sensitive data in production
7. **Optimize Queries**: Use projections and AsNoTracking where appropriate
8. **Plan Migrations**: Test migrations thoroughly before production deployment

This EF Core implementation provides a robust, scalable, and secure data access layer for password authentication with CQRS support, optimized configurations, and comprehensive testing capabilities.</content>
<parameter name="filePath">/home/mfaziz/projects/cs/MasLazu.AspNet.Authentication.Password/src/MasLazu.AspNet.Authentication.Password.EfCore/README.md
