using Microsoft.Extensions.DependencyInjection;

namespace MasLazu.AspNet.Authentication.Password.EfCore.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuthenticationPasswordEntityFrameworkCore(this IServiceCollection services)
    {
        return services;
    }
}
