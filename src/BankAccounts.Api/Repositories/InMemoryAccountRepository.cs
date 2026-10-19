using System.Collections.Concurrent;
using BankAccounts.Api.Models;

namespace BankAccounts.Api.Repositories;

public class InMemoryAccountRepository : IAccountRepository
{
    private readonly ConcurrentDictionary<int, Account> _accounts = new();
    private int _nextId;

    public Account? GetById(int id) =>
        _accounts.TryGetValue(id, out var account) ? account : null;

    public Account Add(Account account)
    {
        account.Id = Interlocked.Increment(ref _nextId);
        _accounts[account.Id] = account;
        return account;
    }

    public void Update(Account account) => _accounts[account.Id] = account;

    public bool Delete(int id) => _accounts.TryRemove(id, out _);
}
