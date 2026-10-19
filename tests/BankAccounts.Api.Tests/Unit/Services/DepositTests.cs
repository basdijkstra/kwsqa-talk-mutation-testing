using BankAccounts.Api.Exceptions;

namespace BankAccounts.Api.Tests.Unit.Services;

[TestFixture]
public class DepositTests : AccountServiceTestBase
{
    [TestCase(0.01, Description = "Smallest positive amount in cents")]
    [TestCase(1)]
    [TestCase(1_000_000)]
    public void Deposit_PositiveAmount_IncreasesBalance(decimal amount)
    {
        var account = GivenCheckingAccount(balance: 100m);

        var result = Service.Deposit(account.Id, amount);

        Assert.Multiple(() =>
        {
            Assert.That(result.Balance, Is.EqualTo(100m + amount));
            Assert.That(StoredBalanceOf(account), Is.EqualTo(100m + amount));
        });
    }

    [Test]
    public void Deposit_AmountBelowOneCent_IsAccepted()
    {
        var account = GivenSavingsAccount();

        var result = Service.Deposit(account.Id, 0.001m);

        Assert.That(result.Balance, Is.EqualTo(0.001m));
    }

    [Test]
    public void Deposit_ReturnsTheUpdatedAccount()
    {
        var account = GivenSavingsAccount(balance: 10m);

        var result = Service.Deposit(account.Id, 5m);

        Assert.Multiple(() =>
        {
            Assert.That(result.Id, Is.EqualTo(account.Id));
            Assert.That(result.Type, Is.EqualTo(account.Type));
            Assert.That(result.Balance, Is.EqualTo(15m));
        });
    }

    [Test]
    public void Deposit_Twice_AddsUp()
    {
        var account = GivenSavingsAccount();

        Service.Deposit(account.Id, 10m);
        Service.Deposit(account.Id, 2.50m);

        Assert.That(StoredBalanceOf(account), Is.EqualTo(12.50m));
    }

    [Test]
    public void Deposit_IntoOverdrawnCheckingAccount_ReducesOverdraft()
    {
        var account = GivenCheckingAccount(balance: -2500m);

        Service.Deposit(account.Id, 500m);

        Assert.That(StoredBalanceOf(account), Is.EqualTo(-2000m));
    }

    [TestCase(0, "0")]
    [TestCase(-0.01, "-0.01")]
    [TestCase(-100, "-100")]
    public void Deposit_ZeroOrNegativeAmount_ThrowsInvalidAmount(decimal amount, string amountInMessage)
    {
        var account = GivenCheckingAccount(balance: 100m);

        var ex = Assert.Throws<InvalidAmountException>(() => Service.Deposit(account.Id, amount));

        Assert.Multiple(() =>
        {
            Assert.That(ex!.Message, Is.EqualTo($"Amount must be greater than 0, but was {amountInMessage}."));
            Assert.That(StoredBalanceOf(account), Is.EqualTo(100m));
            Assert.That(Repository.UpdateCount, Is.Zero);
        });
    }

    [Test]
    public void Deposit_UnknownAccount_ThrowsAccountNotFound()
    {
        Assert.Throws<AccountNotFoundException>(() => Service.Deposit(UnknownAccountId, 10m));
    }

    [Test]
    public void Deposit_InvalidAmountAndUnknownAccount_AmountIsCheckedFirst()
    {
        Assert.Throws<InvalidAmountException>(() => Service.Deposit(UnknownAccountId, 0m));
    }

    [Test]
    public void Deposit_ResultAboveDecimalMaxValue_ThrowsAndLeavesBalanceUntouched()
    {
        var account = GivenSavingsAccount(balance: decimal.MaxValue);

        Assert.Throws<OverflowException>(() => Service.Deposit(account.Id, 1m));
        Assert.That(StoredBalanceOf(account), Is.EqualTo(decimal.MaxValue));
    }
}
