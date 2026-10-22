using BankAccounts.Api.Exceptions;
using BankAccounts.Api.Models;

namespace BankAccounts.Api.Tests.Unit.Services;

[TestFixture]
public class WithdrawTests : AccountServiceTestBase
{
    // Checking accounts may go down to -2500. Savings accounts may not go below 0.

    [TestCase(0, 0.01)]
    public void Withdraw_FromChecking_WithinOverdraftLimit_Succeeds(decimal balance, decimal amount)
    {
        var account = GivenCheckingAccount(balance);

        var result = Service.Withdraw(account.Id, amount);

        Assert.Multiple(() =>
        {
            Assert.That(result.Balance, Is.EqualTo(balance - amount));
            Assert.That(StoredBalanceOf(account), Is.EqualTo(balance - amount));
        });
    }

    [TestCase(100, 100, Description = "Lands exactly on zero")]
    public void Withdraw_FromSavings_WithinBalance_Succeeds(decimal balance, decimal amount)
    {
        var account = GivenSavingsAccount(balance);

        var result = Service.Withdraw(account.Id, amount);

        Assert.Multiple(() =>
        {
            Assert.That(result.Balance, Is.EqualTo(balance - amount));
            Assert.That(StoredBalanceOf(account), Is.EqualTo(balance - amount));
        });
    }

    [TestCase(100, 100.01, Description = "One cent more than the balance")]
    [TestCase(0, 0.01, Description = "Empty account")]
    [TestCase(100, 2500, Description = "Checking overdraft does not apply to savings")]
    public void Withdraw_FromSavings_BeyondBalance_ThrowsInsufficientFunds(decimal balance, decimal amount)
    {
        var account = GivenSavingsAccount(balance);

        Assert.Throws<InsufficientFundsException>(() => Service.Withdraw(account.Id, amount));

        Assert.Multiple(() =>
        {
            Assert.That(StoredBalanceOf(account), Is.EqualTo(balance));
            Assert.That(Repository.UpdateCount, Is.Zero);
        });
    }

    [Test]
    public void Withdraw_UnsupportedAccountType_ThrowsArgumentOutOfRange()
    {
        var account = Repository.Seed((AccountType)99, 100m);

        Assert.Throws<ArgumentOutOfRangeException>(() => Service.Withdraw(account.Id, 10m));
    }
}
