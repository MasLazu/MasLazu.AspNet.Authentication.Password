using Microsoft.EntityFrameworkCore;
using MasLazu.AspNet.Framework.EfCore.Data;
using MasLazu.AspNet.Authentication.Password.Domain.Entities;
using System.Reflection;

namespace MasLazu.AspNet.Authentication.Password.EfCore.Data;

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
