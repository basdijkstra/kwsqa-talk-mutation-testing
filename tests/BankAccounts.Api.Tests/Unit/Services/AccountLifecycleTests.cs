using BankAccounts.Api.Exceptions;
using BankAccounts.Api.Models;

namespace BankAccounts.Api.Tests.Unit.Services;

[TestFixture]
public class AccountLifecycleTests : AccountServiceTestBase
{
    [TestCase(AccountType.Checking)]
    [TestCase(AccountType.Savings)]
    public void CreateAccount_StoresAccountOfRequestedTypeWithZeroBalance(AccountType type)
    {
        var account = Service.CreateAccount(type);

        Assert.Multiple(() =>
        {
            Assert.That(account.Type, Is.EqualTo(type));
            Assert.That(account.Balance, Is.EqualTo(0m));

            var stored = Repository.Stored(account.Id);
            Assert.That(stored, Is.Not.Null);
            Assert.That(stored!.Type, Is.EqualTo(type));
            Assert.That(stored.Balance, Is.EqualTo(0m));
        });
    }

    [Test]
    public void CreateAccount_TwoAccounts_GetDifferentIds()
    {
        var first = Service.CreateAccount(AccountType.Checking);
        var second = Service.CreateAccount(AccountType.Checking);

        Assert.That(second.Id, Is.Not.EqualTo(first.Id));
    }

    [Test]
    public void GetAccount_ExistingAccount_ReturnsIt()
    {
        var seeded = GivenSavingsAccount(balance: 123.45m);

        var account = Service.GetAccount(seeded.Id);

        Assert.Multiple(() =>
        {
            Assert.That(account.Id, Is.EqualTo(seeded.Id));
            Assert.That(account.Type, Is.EqualTo(AccountType.Savings));
            Assert.That(account.Balance, Is.EqualTo(123.45m));
        });
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(UnknownAccountId)]
    [TestCase(int.MaxValue)]
    [TestCase(int.MinValue)]
    public void GetAccount_UnknownId_ThrowsAccountNotFound(int id)
    {
        var ex = Assert.Throws<AccountNotFoundException>(() => Service.GetAccount(id));

        Assert.That(ex!.Message, Is.EqualTo($"Account with id {id} was not found."));
    }

    [Test]
    public void DeleteAccount_ExistingAccount_RemovesIt()
    {
        var account = GivenCheckingAccount();

        Service.DeleteAccount(account.Id);

        Assert.That(Repository.Stored(account.Id), Is.Null);
    }

    [Test]
    public void DeleteAccount_LeavesOtherAccountsAlone()
    {
        var toDelete = GivenCheckingAccount();
        var toKeep = GivenSavingsAccount(balance: 10m);

        Service.DeleteAccount(toDelete.Id);

        Assert.That(Repository.Stored(toKeep.Id), Is.Not.Null);
    }

    [Test]
    public void DeleteAccount_UnknownId_ThrowsAccountNotFound()
    {
        var ex = Assert.Throws<AccountNotFoundException>(() => Service.DeleteAccount(UnknownAccountId));

        Assert.That(ex!.Message, Is.EqualTo($"Account with id {UnknownAccountId} was not found."));
    }

    [Test]
    public void DeleteAccount_AlreadyDeleted_ThrowsAccountNotFound()
    {
        var account = GivenCheckingAccount();
        Service.DeleteAccount(account.Id);

        Assert.Throws<AccountNotFoundException>(() => Service.DeleteAccount(account.Id));
    }
}
