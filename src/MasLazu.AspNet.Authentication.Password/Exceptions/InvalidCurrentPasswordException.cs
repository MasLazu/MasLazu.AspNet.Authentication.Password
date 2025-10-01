using System.Net;
using MasLazu.AspNet.Framework.Application.Exceptions;

namespace MasLazu.AspNet.Authentication.Password.Exceptions;

public class InvalidCurrentPasswordException : AppException
{
    public InvalidCurrentPasswordException()
        : base(
            message: "Current password is incorrect.",
            errorCode: "PASSWORD_AUTH_INVALID_CURRENT_PASSWORD",
            statusCode: HttpStatusCode.Unauthorized)
    {
    }
}
