using FluentValidation;
using MasLazu.AspNet.Authentication.Password.Configurations;

namespace MasLazu.AspNet.Authentication.Password.Validators;

public class PasswordLoginMethodConfigurationValidator : AbstractValidator<PasswordLoginMethodConfiguration>
{
    public PasswordLoginMethodConfigurationValidator()
    {
        RuleFor(x => x.PasswordValidation)
            .NotNull()
            .WithMessage("PasswordValidation configuration is required.");

        When(x => x.PasswordValidation != null, () => RuleFor(x => x.PasswordValidation.MinLength)
                .GreaterThan(0)
                .WithMessage("Password minimum length must be greater than 0.")
                .LessThanOrEqualTo(128)
                .WithMessage("Password minimum length cannot exceed 128 characters."));
    }
}
