using System.Net;
using MasLazu.AspNet.Framework.Application.Exceptions;

namespace MasLazu.AspNet.Authentication.Password.Exceptions;

public class EmailAlreadyTakenException : AppException
{
    public EmailAlreadyTakenException(string email)
        : base(
            message: "Email is already taken.",
            errorCode: "PASSWORD_AUTH_EMAIL_TAKEN",
            statusCode: HttpStatusCode.Conflict,
            details: new { Email = email })
    {
    }
}
