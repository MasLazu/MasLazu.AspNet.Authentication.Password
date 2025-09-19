using MasLazu.AspNet.Framework.Domain.Entities;

namespace MasLazu.AspNet.Authentication.Password.Domain.Entities;

public class UserPasswordLogin : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid UserLoginMethodId { get; set; }
    public bool IsVerified { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime? LastLoginDate { get; set; }
}
