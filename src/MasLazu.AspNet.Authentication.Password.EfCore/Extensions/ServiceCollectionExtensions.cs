using MasLazu.AspNet.Authentication.Password.Domain.Entities;
using MasLazu.AspNet.Authentication.Password.EfCore.Data;
using MasLazu.AspNet.Framework.Application.Interfaces;
using MasLazu.AspNet.Framework.EfCore.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace MasLazu.AspNet.Authentication.Password.EfCore.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuthenticationPasswordEntityFrameworkCore(this IServiceCollection services)
    {
        services.AddScoped<IRepository<UserPasswordLogin>, Repository<UserPasswordLogin, PasswordDbContext>>();
        services.AddScoped<IReadRepository<UserPasswordLogin>, ReadRepository<UserPasswordLogin, PasswordReadDbContext>>();

        return services;
    }
}
