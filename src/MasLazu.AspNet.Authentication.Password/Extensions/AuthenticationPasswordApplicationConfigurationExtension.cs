using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MasLazu.AspNet.Authentication.Password.Configurations;
using MasLazu.AspNet.Authentication.Password.Validators;

namespace MasLazu.AspNet.Authentication.Password.Extensions;

public static class AuthenticationPasswordApplicationConfigurationExtension
{
    public static IServiceCollection AddAuthenticationPasswordApplicationConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<PasswordLoginMethodConfiguration>()
            .Bind(configuration.GetSection("PasswordLoginMethod"))
            .Validate(config =>
            {
                var validator = new PasswordLoginMethodConfigurationValidator();
                ValidationResult result = validator.Validate(config);
                if (!result.IsValid)
                {
                    string errors = string.Join("; ", result.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"));
                    throw new ValidationException($"PasswordLoginMethod configuration validation failed: {errors}");
                }
                return true;
            })
            .ValidateOnStart();

        return services;
    }
}
