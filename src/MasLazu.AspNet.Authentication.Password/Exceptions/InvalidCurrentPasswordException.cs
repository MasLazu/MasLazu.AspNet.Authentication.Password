using System.Net;
using MasLazu.AspNet.Framework.Application.Exceptions;

namespace MasLazu.AspNet.Authentication.Password.Exceptions;

public class InvalidCurrentPasswordException : AppException
{
    public InvalidCurrentPasswordException()
        : base(
            message: "Current password is incorrect.",
            errorCode: "password_auth_invalid_current_password",
            statusCode: HttpStatusCode.Unauthorized)
    {
    }
}
