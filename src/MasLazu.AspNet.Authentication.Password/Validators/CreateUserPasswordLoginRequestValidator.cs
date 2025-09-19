using FluentValidation;
using MasLazu.AspNet.Authentication.Password.Abstraction.Models;

namespace MasLazu.AspNet.Authentication.Password.Validators;

public class CreateUserPasswordLoginRequestValidator : AbstractValidator<CreateUserPasswordLoginRequest>
{
    public CreateUserPasswordLoginRequestValidator()
    {
        RuleFor(x => x.UserLoginMethodId)
            .NotEmpty();

        RuleFor(x => x.PasswordHash)
            .NotEmpty()
            .MaximumLength(255);
    }
}
