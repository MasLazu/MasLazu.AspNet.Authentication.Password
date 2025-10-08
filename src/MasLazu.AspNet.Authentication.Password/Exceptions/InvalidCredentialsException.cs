using System.Net;
using MasLazu.AspNet.Framework.Application.Exceptions;

namespace MasLazu.AspNet.Authentication.Password.Exceptions;

public class InvalidCredentialsException : AppException
{
    public InvalidCredentialsException(string? identifier = null)
        : base(
            message: "Invalid username/email or password.",
            errorCode: "password_auth_invalid_credentials",
            statusCode: HttpStatusCode.Unauthorized,
            details: identifier != null ? new { Identifier = identifier } : null)
    {
    }
}
