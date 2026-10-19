using BankAccounts.Api.Models;
using BankAccounts.Api.Services;
using BankAccounts.Api.Tests.Unit.Support;

namespace BankAccounts.Api.Tests.Unit.Services;

public abstract class AccountServiceTestBase
{
    protected const int UnknownAccountId = 999;

    protected FakeAccountRepository Repository { get; private set; } = null!;

    protected AccountService Service { get; private set; } = null!;

    [SetUp]
    public void CreateService()
    {
        Repository = new FakeAccountRepository();
        Service = new AccountService(Repository);
    }

    protected Account GivenCheckingAccount(decimal balance = 0m) =>
        Repository.Seed(AccountType.Checking, balance);

    protected Account GivenSavingsAccount(decimal balance = 0m) =>
        Repository.Seed(AccountType.Savings, balance);

    protected decimal StoredBalanceOf(Account account) =>
        Repository.Stored(account.Id)!.Balance;
}
