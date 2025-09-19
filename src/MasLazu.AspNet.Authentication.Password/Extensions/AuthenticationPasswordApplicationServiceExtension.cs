using Microsoft.Extensions.DependencyInjection;
using MasLazu.AspNet.Authentication.Password.Services;
using MasLazu.AspNet.Authentication.Password.Abstraction.Interfaces;

namespace MasLazu.AspNet.Authentication.Password.Extensions;

public static class AuthenticationPasswordApplicationServiceExtension
{
    public static IServiceCollection AddAuthenticationPasswordApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IUserPasswordLoginService, UserPasswordLoginService>();

        return services;
    }
}
