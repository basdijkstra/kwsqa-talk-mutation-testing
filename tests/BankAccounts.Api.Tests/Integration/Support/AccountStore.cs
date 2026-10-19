using BankAccounts.Api.Models;
using BankAccounts.Api.Repositories;

namespace BankAccounts.Api.Tests.Integration.Support;

/// <summary>
/// Direct access to the API's data store. Use it to set up accounts with a given
/// balance without calling other endpoints, and to check what the API actually saved.
/// </summary>
public class AccountStore(IAccountRepository repository)
{
    public Account Seed(AccountType type, decimal balance = 0m) =>
        repository.Add(new Account { Type = type, Balance = balance });

    public Account SeedChecking(decimal balance = 0m) => Seed(AccountType.Checking, balance);

    public Account? Find(int id) => repository.GetById(id);

    public decimal BalanceOf(int id) =>
        Find(id)?.Balance ?? throw new InvalidOperationException($"Account {id} is not in the store.");
}
