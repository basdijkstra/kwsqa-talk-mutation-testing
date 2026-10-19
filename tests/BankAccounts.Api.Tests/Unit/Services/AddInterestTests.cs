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
}
