namespace MasLazu.AspNet.Authentication.Password.Abstraction.Models;

/// <summary>
/// Request model for password-based login
/// </summary>
public record PasswordLoginRequest(
    string Identifier,
    string Password
);
