using System.Net;
using MasLazu.AspNet.Framework.Application.Exceptions;

namespace MasLazu.AspNet.Authentication.Password.Exceptions;

public class UsernameAlreadyTakenException : AppException
{
    public UsernameAlreadyTakenException(string username)
        : base(
            message: "Username is already taken.",
            errorCode: "PASSWORD_AUTH_USERNAME_TAKEN",
            statusCode: HttpStatusCode.Conflict,
            details: new { Username = username })
    {
    }
}
