namespace MasLazu.AspNet.Authentication.Password.Abstraction.Models;

public record PasswordRegisterRequest(
    string Name,
    string Username,
    string Email,
    string Password
);
