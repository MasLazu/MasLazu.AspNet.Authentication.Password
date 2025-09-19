namespace MasLazu.AspNet.Authentication.Password.Abstraction.Models;

public record PasswordLoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiresAt,
    DateTimeOffset RefreshTokenExpiresAt,
    string TokenType = "Bearer"
);
