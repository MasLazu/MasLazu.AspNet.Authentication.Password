namespace MasLazu.AspNet.Authentication.Password.Abstraction.Models;

public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword
);
