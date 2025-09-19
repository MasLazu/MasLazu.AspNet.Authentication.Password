using FluentValidation;
using Microsoft.Extensions.Options;
using MasLazu.AspNet.Authentication.Password.Abstraction.Models;
using MasLazu.AspNet.Authentication.Password.Configurations;

namespace MasLazu.AspNet.Authentication.Password.Validators;

public class PasswordRegisterRequestValidator : AbstractValidator<PasswordRegisterRequest>
{
    public PasswordRegisterRequestValidator(IOptions<PasswordLoginMethodConfiguration> passwordConfig)
    {
        PasswordValidationConfiguration config = passwordConfig.Value.PasswordValidation;

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required.")
            .MaximumLength(100)
            .WithMessage("Name must not exceed 100 characters.");

        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Username is required.")
            .MaximumLength(50)
            .WithMessage("Username must not exceed 50 characters.")
            .Matches("^[a-zA-Z0-9_]+$")
            .WithMessage("Username can only contain letters, numbers, and underscores.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Invalid email format.")
            .MaximumLength(255)
            .WithMessage("Email must not exceed 255 characters.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required.")
            .MinimumLength(config.MinLength)
            .WithMessage($"Password must be at least {config.MinLength} characters long.");

        if (config.RequireUppercase)
        {
            RuleFor(x => x.Password)
                .Matches("[A-Z]")
                .WithMessage("Password must contain at least one uppercase letter.");
        }

        if (config.RequireLowercase)
        {
            RuleFor(x => x.Password)
                .Matches("[a-z]")
                .WithMessage("Password must contain at least one lowercase letter.");
        }

        if (config.RequireDigit)
        {
            RuleFor(x => x.Password)
                .Matches("[0-9]")
                .WithMessage("Password must contain at least one digit.");
        }

        if (config.RequireSpecialCharacter)
        {
            RuleFor(x => x.Password)
                .Matches("[^a-zA-Z0-9]")
                .WithMessage("Password must contain at least one special character.");
        }
    }
}
