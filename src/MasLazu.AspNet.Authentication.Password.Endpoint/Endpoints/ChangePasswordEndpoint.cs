using FastEndpoints;
using MasLazu.AspNet.Framework.Endpoint.Endpoints;
using MasLazu.AspNet.Authentication.Password.Abstraction.Interfaces;
using MasLazu.AspNet.Authentication.Password.Abstraction.Models;
using System.Security.Claims;
using MasLazu.AspNet.Authentication.Password.Endpoint.EndpointGroups;
using MasLazu.AspNet.Framework.Application.Exceptions;

namespace MasLazu.AspNet.Authentication.Password.Endpoint.Endpoints;

public class ChangePasswordEndpoint : BaseEndpointWithoutResponse<ChangePasswordRequest>
{
    public IUserPasswordLoginService UserPasswordLoginService { get; set; }

    public override void ConfigureEndpoint()
    {
        Post("/change-password");
        Group<AuthEndpointGroup>();
    }

    public override async Task HandleAsync(ChangePasswordRequest req, CancellationToken ct)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
        {
            throw new UnauthorizedException("User is not authenticated");
        }

        await UserPasswordLoginService.ChangePasswordAsync(userId, req, ct);
        await SendOkResponseAsync("Change Password Successful", ct);
    }
}
