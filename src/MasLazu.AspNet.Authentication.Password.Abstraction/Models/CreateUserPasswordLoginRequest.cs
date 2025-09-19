namespace MasLazu.AspNet.Authentication.Password.Abstraction.Models;

public record CreateUserPasswordLoginRequest(
    Guid UserLoginMethodId,
    string PasswordHash
);
