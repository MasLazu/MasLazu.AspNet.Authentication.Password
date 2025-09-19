using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MasLazu.AspNet.Authentication.Core.Abstraction.Interfaces;
using MasLazu.AspNet.Authentication.Core.Abstraction.Models;

namespace MasLazu.AspNet.Authentication.Password.BackgroundServices;

public class AuthenticationPasswordDatabaseSeedBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public AuthenticationPasswordDatabaseSeedBackgroundService(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using IServiceScope scope = _serviceScopeFactory.CreateScope();
        ILoginMethodService loginMethodService = scope.ServiceProvider.GetRequiredService<ILoginMethodService>();

        var passwordLoginMethodId = Guid.Parse("019950d6-5fb1-74ad-a33e-2e620dc842c0");
        var createRequest = new CreateLoginMethodRequest("password");
        await loginMethodService.CreateIfNotExistsAsync(passwordLoginMethodId, createRequest, stoppingToken);
    }
}
