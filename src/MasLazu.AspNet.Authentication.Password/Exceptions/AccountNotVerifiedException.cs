using System.Net;
using MasLazu.AspNet.Framework.Application.Exceptions;

namespace MasLazu.AspNet.Authentication.Password.Exceptions;

public class AccountNotVerifiedException : AppException
{
    public AccountNotVerifiedException(string? email = null)
        : base(
            message: "Account not verified. Please verify your account before logging in.",
            errorCode: "password_auth_account_not_verified",
            statusCode: HttpStatusCode.Forbidden,
            details: email != null ? new { Email = email } : null)
    {
    }
}
