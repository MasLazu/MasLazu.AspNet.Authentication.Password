using FastEndpoints;
using MasLazu.AspNet.Framework.Endpoint.Endpoints;
using MasLazu.AspNet.Authentication.Password.Abstraction.Interfaces;
using MasLazu.AspNet.Authentication.Password.Abstraction.Models;
using MasLazu.AspNet.Authentication.Password.Endpoint.EndpointGroups;

namespace MasLazu.AspNet.Authentication.Password.Endpoint.Endpoints;

public class RegisterEndpoint : BaseEndpointWithoutResponse<PasswordRegisterRequest>
{
    public IUserPasswordLoginService UserPasswordLoginService { get; set; }

    public override void ConfigureEndpoint()
    {
        Post("/register");
        Group<AuthEndpointGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(PasswordRegisterRequest req, CancellationToken ct)
    {
        await UserPasswordLoginService.RegisterAsync(req, ct);
        await SendOkResponseAsync("Register Successful", ct);
    }
}
