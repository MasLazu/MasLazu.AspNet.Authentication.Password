using FastEndpoints;
using MasLazu.AspNet.Framework.Endpoint.Endpoints;
using MasLazu.AspNet.Authentication.Password.Abstraction.Interfaces;
using MasLazu.AspNet.Authentication.Password.Abstraction.Models;
using MasLazu.AspNet.Authentication.Password.Endpoint.EndpointGroups;

namespace MasLazu.AspNet.Authentication.Password.Endpoint.Endpoints;

public class LoginEndpoint : BaseEndpoint<PasswordLoginRequest, PasswordLoginResponse>
{
    public IUserPasswordLoginService UserPasswordLoginService { get; set; }

    public override void ConfigureEndpoint()
    {
        Post("/login");
        Group<AuthEndpointGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(PasswordLoginRequest req, CancellationToken ct)
    {
        PasswordLoginResponse response = await UserPasswordLoginService.LoginAsync(req, ct);
        await SendOkResponseAsync(response, "Login Successful", ct);
    }
}
