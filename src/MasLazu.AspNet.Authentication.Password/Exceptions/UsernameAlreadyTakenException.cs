using System.Net;
using MasLazu.AspNet.Framework.Application.Exceptions;

namespace MasLazu.AspNet.Authentication.Password.Exceptions;

public class UsernameAlreadyTakenException : AppException
{
    public UsernameAlreadyTakenException(string username)
        : base(
            message: "Username is already taken.",
            errorCode: "password_auth_username_taken",
            statusCode: HttpStatusCode.Conflict,
            details: new { Username = username })
    {
    }
}
