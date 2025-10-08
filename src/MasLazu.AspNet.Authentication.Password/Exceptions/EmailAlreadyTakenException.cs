using System.Net;
using MasLazu.AspNet.Framework.Application.Exceptions;

namespace MasLazu.AspNet.Authentication.Password.Exceptions;

public class EmailAlreadyTakenException : AppException
{
    public EmailAlreadyTakenException(string email)
        : base(
            message: "Email is already taken.",
            errorCode: "password_auth_email_taken",
            statusCode: HttpStatusCode.Conflict,
            details: new { Email = email })
    {
    }
}
