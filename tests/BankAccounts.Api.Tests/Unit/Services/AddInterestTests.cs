using BankAccounts.Api.Exceptions;

namespace BankAccounts.Api.Tests.Unit.Services;

[TestFixture]
public class AddInterestTests : AccountServiceTestBase
{
    // Interest is 2% of the balance, rounded to cents.

    [TestCase(100, 102)]
    [TestCase(1000, 1020)]
    [TestCase(0, 0, Description = "Empty account earns nothing")]
    [TestCase(0.50, 0.51, Description = "Smallest balance that earns exactly one cent")]
    [TestCase(1234.56, 1259.25, Description = "24.6912 is rounded down to 24.69")]
    public void AddInterest_SavingsAccount_AddsTwoPercent(decimal balance, decimal expected)
    {
        var account = GivenSavingsAccount(balance);

        var result = Service.AddInterest(account.Id);

        Assert.Multiple(() =>
        {
            Assert.That(result.Balance, Is.EqualTo(expected));
            Assert.That(StoredBalanceOf(account), Is.EqualTo(expected));
        });
    }

    [TestCase(0.24, 0.24, Description = "0.0048 rounds down to 0.00")]
    [TestCase(0.26, 0.27, Description = "0.0052 rounds up to 0.01")]
    [TestCase(0.74, 0.75, Description = "0.0148 rounds down to 0.01")]
    [TestCase(0.76, 0.78, Description = "0.0152 rounds up to 0.02")]
    public void AddInterest_RoundsInterestToNearestCent(decimal balance, decimal expected)
    {
        var account = GivenSavingsAccount(balance);

        var result = Service.AddInterest(account.Id);

        Assert.That(result.Balance, Is.EqualTo(expected));
    }

    // Math.Round uses banker's rounding: an exact half cent goes to the even cent.
    [TestCase(0.25, 0.25, Description = "0.005 rounds to 0.00")]
    [TestCase(0.75, 0.77, Description = "0.015 rounds to 0.02")]
    [TestCase(1.25, 1.27, Description = "0.025 rounds to 0.02")]
    public void AddInterest_HalfCent_RoundsToEven(decimal balance, decimal expected)
    {
        var account = GivenSavingsAccount(balance);

        var result = Service.AddInterest(account.Id);

        Assert.That(result.Balance, Is.EqualTo(expected));
    }

    [Test]
    public void AddInterest_Twice_Compounds()
    {
        var account = GivenSavingsAccount(balance: 100m);

        Service.AddInterest(account.Id);
        Service.AddInterest(account.Id);

        Assert.That(StoredBalanceOf(account), Is.EqualTo(104.04m));
    }

    [Test]
    public void AddInterest_ReturnsTheUpdatedAccount()
    {
        var account = GivenSavingsAccount(balance: 50m);

        var result = Service.AddInterest(account.Id);

        Assert.Multiple(() =>
        {
            Assert.That(result.Id, Is.EqualTo(account.Id));
            Assert.That(result.Type, Is.EqualTo(account.Type));
            Assert.That(result.Balance, Is.EqualTo(51m));
        });
    }

    [TestCase(0)]
    [TestCase(100)]
    [TestCase(-100)]
    public void AddInterest_CheckingAccount_ThrowsInterestNotAllowed(decimal balance)
    {
        var account = GivenCheckingAccount(balance);

        var ex = Assert.Throws<InterestNotAllowedException>(() => Service.AddInterest(account.Id));

        Assert.Multiple(() =>
        {
            Assert.That(ex!.Message, Is.EqualTo(
                $"Interest can only be added to savings accounts, but account {account.Id} is of type CHECKING."));
            Assert.That(StoredBalanceOf(account), Is.EqualTo(balance));
            Assert.That(Repository.UpdateCount, Is.Zero);
        });
    }

    [Test]
    public void AddInterest_UnknownAccount_ThrowsAccountNotFound()
    {
        Assert.Throws<AccountNotFoundException>(() => Service.AddInterest(UnknownAccountId));
    }
}
