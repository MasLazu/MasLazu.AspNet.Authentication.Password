using Microsoft.Extensions.DependencyInjection;
using MasLazu.AspNet.Framework.Application.Interfaces;
using MasLazu.AspNet.Authentication.Password.Utils;

namespace MasLazu.AspNet.Authentication.Password.Extensions;

public static class AuthenticationPasswordApplicationUtilExtension
{
    public static IServiceCollection AddAuthenticationPasswordApplicationUtils(this IServiceCollection services)
    {
        services.AddScoped<IEntityPropertyMap<Domain.Entities.UserPasswordLogin>, UserPasswordLoginEntityPropertyMap>();

        return services;
    }
}
