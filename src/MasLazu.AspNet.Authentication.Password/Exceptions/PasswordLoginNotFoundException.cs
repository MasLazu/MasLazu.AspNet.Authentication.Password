using System.Net;
using MasLazu.AspNet.Framework.Application.Exceptions;

namespace MasLazu.AspNet.Authentication.Password.Exceptions;

public class PasswordLoginNotFoundException : AppException
{
    public PasswordLoginNotFoundException(Guid userId)
        : base(
            message: "No password login found for the specified user.",
            errorCode: "PASSWORD_AUTH_LOGIN_NOT_FOUND",
            statusCode: HttpStatusCode.NotFound,
            details: new { UserId = userId })
    {
    }
}
