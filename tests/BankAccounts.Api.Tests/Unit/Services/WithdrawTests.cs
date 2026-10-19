using BankAccounts.Api.Exceptions;
using BankAccounts.Api.Models;

namespace BankAccounts.Api.Tests.Unit.Services;

[TestFixture]
public class WithdrawTests : AccountServiceTestBase
{
    // Checking accounts may go down to -2500. Savings accounts may not go below 0.

    [TestCase(0, 0.01)]
    [TestCase(0, 2500, Description = "Lands exactly on the overdraft limit")]
    [TestCase(100, 2600, Description = "Lands exactly on the overdraft limit from a positive balance")]
    [TestCase(-2499.99, 0.01, Description = "Lands exactly on the overdraft limit from an overdrawn balance")]
    [TestCase(100, 100)]
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

    [TestCase(0, 2500.01, Description = "One cent past the overdraft limit")]
    [TestCase(100, 2600.01)]
    [TestCase(-2500, 0.01, Description = "Already at the limit")]
    [TestCase(0, 1_000_000)]
    public void Withdraw_FromChecking_BeyondOverdraftLimit_ThrowsInsufficientFunds(decimal balance, decimal amount)
    {
        var account = GivenCheckingAccount(balance);

        Assert.Throws<InsufficientFundsException>(() => Service.Withdraw(account.Id, amount));

        Assert.Multiple(() =>
        {
            Assert.That(StoredBalanceOf(account), Is.EqualTo(balance));
            Assert.That(Repository.UpdateCount, Is.Zero);
        });
    }

    [TestCase(100, 0.01)]
    [TestCase(100, 99.99)]
    [TestCase(100, 100, Description = "Lands exactly on zero")]
    [TestCase(0.01, 0.01, Description = "Lands exactly on zero from the smallest balance")]
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
    public void Withdraw_InsufficientFunds_MessageContainsAccountBalanceAndAmount()
    {
        var account = GivenSavingsAccount(balance: 100.50m);

        var ex = Assert.Throws<InsufficientFundsException>(() => Service.Withdraw(account.Id, 100.51m));

        Assert.That(ex!.Message, Is.EqualTo(
            $"Withdrawal of 100.51 from account {account.Id} with balance 100.50 exceeds the allowed overdraft."));
    }

    [Test]
    public void Withdraw_ReturnsTheUpdatedAccount()
    {
        var account = GivenCheckingAccount(balance: 10m);

        var result = Service.Withdraw(account.Id, 4m);

        Assert.Multiple(() =>
        {
            Assert.That(result.Id, Is.EqualTo(account.Id));
            Assert.That(result.Type, Is.EqualTo(AccountType.Checking));
            Assert.That(result.Balance, Is.EqualTo(6m));
        });
    }

    [Test]
    public void Withdraw_Twice_UsesBalanceAfterFirstWithdrawal()
    {
        var account = GivenSavingsAccount(balance: 100m);
        Service.Withdraw(account.Id, 60m);

        Assert.Throws<InsufficientFundsException>(() => Service.Withdraw(account.Id, 40.01m));
        Assert.That(StoredBalanceOf(account), Is.EqualTo(40m));
    }

    [TestCase(AccountType.Checking, 0)]
    [TestCase(AccountType.Checking, -0.01)]
    [TestCase(AccountType.Savings, 0)]
    [TestCase(AccountType.Savings, -50)]
    public void Withdraw_ZeroOrNegativeAmount_ThrowsInvalidAmount(AccountType type, decimal amount)
    {
        var account = Repository.Seed(type, 100m);

        Assert.Throws<InvalidAmountException>(() => Service.Withdraw(account.Id, amount));

        Assert.Multiple(() =>
        {
            Assert.That(StoredBalanceOf(account), Is.EqualTo(100m));
            Assert.That(Repository.UpdateCount, Is.Zero);
        });
    }

    [Test]
    public void Withdraw_UnknownAccount_ThrowsAccountNotFound()
    {
        Assert.Throws<AccountNotFoundException>(() => Service.Withdraw(UnknownAccountId, 10m));
    }

    [Test]
    public void Withdraw_InvalidAmountAndUnknownAccount_AmountIsCheckedFirst()
    {
        Assert.Throws<InvalidAmountException>(() => Service.Withdraw(UnknownAccountId, -1m));
    }

    [Test]
    public void Withdraw_UnsupportedAccountType_ThrowsArgumentOutOfRange()
    {
        var account = Repository.Seed((AccountType)99, 100m);

        Assert.Throws<ArgumentOutOfRangeException>(() => Service.Withdraw(account.Id, 10m));
    }
}
