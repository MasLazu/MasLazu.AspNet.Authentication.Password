using System.Linq.Expressions;
using MasLazu.AspNet.Framework.Application.Interfaces;

namespace MasLazu.AspNet.Authentication.Password.Utils;

public class UserPasswordLoginEntityPropertyMap : IEntityPropertyMap<Domain.Entities.UserPasswordLogin>
{
    private readonly Dictionary<string, Expression<Func<Domain.Entities.UserPasswordLogin, object>>> _map =
        new(StringComparer.OrdinalIgnoreCase)
        {
            { "id", upl => upl.Id },
            { "userLoginMethodId", upl => upl.UserLoginMethodId },
            { "passwordHash", upl => upl.PasswordHash },
            { "lastLoginDate", upl => upl.LastLoginDate! },
            { "createdAt", upl => upl.CreatedAt },
            { "updatedAt", upl => upl.UpdatedAt! }
        };

    public Expression<Func<Domain.Entities.UserPasswordLogin, object>> Get(string property)
    {
        if (_map.TryGetValue(property, out Expression<Func<Domain.Entities.UserPasswordLogin, object>>? expr))
        {
            return expr;
        }

        throw new ArgumentException($"Property '{property}' is not supported for UserPasswordLogin. " +
            $"Supported properties: {string.Join(", ", _map.Keys)}");
    }
}
