using BankAccounts.Api.Repositories;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace BankAccounts.Api.Tests.Integration.Support;

/// <summary>
/// Hosts the real API in memory. Every instance has its own repository,
/// so tests that create their own factory never share data.
/// </summary>
public class BankAccountsApiFactory : WebApplicationFactory<Program>
{
    public IAccountRepository Repository => Services.GetRequiredService<IAccountRepository>();
}
