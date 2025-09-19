using Microsoft.Extensions.DependencyInjection;

namespace MasLazu.AspNet.Authentication.Password.Endpoint.Extensions;

public static class PasswordEndpointExtension
{
    public static IServiceCollection AddPasswordEndpoints(this IServiceCollection services)
    {
        return services;
    }
}
