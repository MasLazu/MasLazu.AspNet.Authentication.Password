using Microsoft.Extensions.DependencyInjection;
using MasLazu.AspNet.Authentication.Password.BackgroundServices;

namespace MasLazu.AspNet.Authentication.Password.Extensions;

public static class AuthenticationPasswordApplicationBackgroundServiceExtension
{
    public static IServiceCollection AddDatabaseSeedBackgroundService(this IServiceCollection services)
    {
        services.AddHostedService<AuthenticationPasswordDatabaseSeedBackgroundService>();
        return services;
    }
}
