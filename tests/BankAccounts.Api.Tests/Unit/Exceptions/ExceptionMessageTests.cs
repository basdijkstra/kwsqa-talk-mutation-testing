using BankAccounts.Api.Exceptions;
using BankAccounts.Api.Models;

namespace BankAccounts.Api.Tests.Unit.Exceptions;

/// <summary>
/// Exception messages end up in API responses, so their wording is part of the contract.
/// The Dutch culture uses a comma as decimal separator, which checks that amounts are
/// always formatted with a dot.
/// </summary>
[TestFixture]
[SetCulture("nl-NL")]
public class ExceptionMessageTests
{
    [Test]
    public void AccountNotFound()
    {
        var ex = new AccountNotFoundException(7);

        Assert.That(ex.Message, Is.EqualTo("Account with id 7 was not found."));
    }

    [Test]
    public void InvalidAmount_UsesInvariantCulture()
    {
        var ex = new InvalidAmountException(-12.5m);

        Assert.That(ex.Message, Is.EqualTo("Amount must be greater than 0, but was -12.5."));
    }

    [Test]
    public void InsufficientFunds_UsesInvariantCulture()
    {
        var ex = new InsufficientFundsException(3, -2499.99m, 1000.01m);

        Assert.That(ex.Message, Is.EqualTo(
            "Withdrawal of 1000.01 from account 3 with balance -2499.99 exceeds the allowed overdraft."));
    }

    [TestCase(AccountType.Checking, "CHECKING")]
    [TestCase(AccountType.Savings, "SAVINGS")]
    public void InterestNotAllowed_ShowsTypeInUpperCase(AccountType type, string expectedType)
    {
        var ex = new InterestNotAllowedException(5, type);

        Assert.That(ex.Message, Is.EqualTo(
            $"Interest can only be added to savings accounts, but account 5 is of type {expectedType}."));
    }
}
