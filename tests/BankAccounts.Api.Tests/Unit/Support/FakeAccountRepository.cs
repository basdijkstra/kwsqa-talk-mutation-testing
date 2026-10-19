using BankAccounts.Api.Models;
using BankAccounts.Api.Repositories;

namespace BankAccounts.Api.Tests.Unit.Support;

/// <summary>
/// Test double for <see cref="IAccountRepository"/>. It stores and hands out copies,
/// so a change to an account only becomes visible after the service calls Update.
/// </summary>
public class FakeAccountRepository : IAccountRepository
{
    private readonly Dictionary<int, Account> _accounts = new();
    private int _nextId;

    public Account? GetById(int id) =>
        _accounts.TryGetValue(id, out var account) ? Copy(account) : null;

    public Account Add(Account account)
    {
        account.Id = ++_nextId;
        _accounts[account.Id] = Copy(account);
        return account;
    }

    public void Update(Account account) => _accounts[account.Id] = Copy(account);

    public bool Delete(int id) => _accounts.Remove(id);

    /// <summary>Puts an account with a given state straight into storage, bypassing the service.</summary>
    public Account Seed(AccountType type, decimal balance)
    {
        var account = Add(new Account { Type = type, Balance = balance });
        return Copy(account);
    }

    /// <summary>Reads what is stored, without going through the service.</summary>
    public Account? Stored(int id) => GetById(id);

    private static Account Copy(Account account) =>
        new() { Id = account.Id, Type = account.Type, Balance = account.Balance };
}
