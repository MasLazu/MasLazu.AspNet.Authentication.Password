using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MasLazu.AspNet.Authentication.Password.Extensions;

public static class AuthenticationPasswordApplicationExtension
{
    public static IServiceCollection AddAuthenticationPasswordApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthenticationPasswordApplicationConfiguration(configuration);
        services.AddAuthenticationPasswordApplicationServices();
        services.AddAuthenticationPasswordApplicationUtils();
        services.AddAuthenticationPasswordApplicationValidators();
        services.AddDatabaseSeedBackgroundService();

        return services;
    }
}
