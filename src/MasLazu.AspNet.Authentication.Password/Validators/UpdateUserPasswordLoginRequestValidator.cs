using FluentValidation;
using MasLazu.AspNet.Authentication.Password.Abstraction.Models;

namespace MasLazu.AspNet.Authentication.Password.Validators;

public class UpdateUserPasswordLoginRequestValidator : AbstractValidator<UpdateUserPasswordLoginRequest>
{
    public UpdateUserPasswordLoginRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.PasswordHash)
            .MaximumLength(255)
            .When(x => !string.IsNullOrEmpty(x.PasswordHash));
    }
}
