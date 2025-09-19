namespace MasLazu.AspNet.Authentication.Password.Configurations;

public class PasswordValidationConfiguration
{
    public int MinLength { get; set; } = 8;
    public bool RequireUppercase { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireDigit { get; set; } = true;
    public bool RequireSpecialCharacter { get; set; } = false;
}

public class PasswordLoginMethodConfiguration
{
    public bool RequireVerification { get; set; } = true;
    public PasswordValidationConfiguration PasswordValidation { get; set; } = new();
}
