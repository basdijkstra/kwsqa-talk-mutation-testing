namespace BankAccounts.Api.Tests.Unit.Services;

[TestFixture]
public class AddInterestTests : AccountServiceTestBase
{
    // Interest is 2% of the balance, rounded to cents.

    [TestCase(100, 102)]
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
}
