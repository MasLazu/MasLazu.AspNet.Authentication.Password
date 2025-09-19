using FluentValidation;
using MasLazu.AspNet.Authentication.Password.Abstraction.Models;

namespace MasLazu.AspNet.Authentication.Password.Validators;

public class PasswordLoginRequestValidator : AbstractValidator<PasswordLoginRequest>
{
    public PasswordLoginRequestValidator()
    {
        RuleFor(x => x.Identifier)
            .NotEmpty()
            .WithMessage("Identifier is required.")
            .MaximumLength(255)
            .WithMessage("Identifier must not exceed 255 characters.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required.");
    }
}
