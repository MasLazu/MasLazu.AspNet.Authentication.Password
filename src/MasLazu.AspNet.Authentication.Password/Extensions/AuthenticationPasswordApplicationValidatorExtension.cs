using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MasLazu.AspNet.Authentication.Password.Abstraction.Models;
using MasLazu.AspNet.Authentication.Password.Validators;

namespace MasLazu.AspNet.Authentication.Password.Extensions;

public static class AuthenticationPasswordApplicationValidatorExtension
{
    public static IServiceCollection AddAuthenticationPasswordApplicationValidators(this IServiceCollection services)
    {
        services.AddScoped<IValidator<CreateUserPasswordLoginRequest>, CreateUserPasswordLoginRequestValidator>();
        services.AddScoped<IValidator<UpdateUserPasswordLoginRequest>, UpdateUserPasswordLoginRequestValidator>();
        services.AddScoped<IValidator<PasswordLoginRequest>, PasswordLoginRequestValidator>();
        services.AddScoped<IValidator<PasswordRegisterRequest>, PasswordRegisterRequestValidator>();

        return services;
    }
}
