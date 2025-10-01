using System.Net;
using MasLazu.AspNet.Framework.Application.Exceptions;

namespace MasLazu.AspNet.Authentication.Password.Exceptions;

public class AccountNotVerifiedException : AppException
{
    public AccountNotVerifiedException(string? email = null)
        : base(
            message: "Account not verified. Please verify your account before logging in.",
            errorCode: "PASSWORD_AUTH_ACCOUNT_NOT_VERIFIED",
            statusCode: HttpStatusCode.Forbidden,
            details: email != null ? new { Email = email } : null)
    {
    }
}
