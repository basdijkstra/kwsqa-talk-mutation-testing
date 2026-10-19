using BankAccounts.Api.Tests.Integration.Support;

namespace BankAccounts.Api.Tests.Integration.Endpoints;

[TestFixture]
public class AddInterestTests : ApiTestBase
{
    [TestCase(100, 102)]
    [TestCase(0, 0, Description = "Empty account earns nothing")]
    [TestCase(0.26, 0.27, Description = "0.0052 rounds up to 0.01")]
    [TestCase(0.24, 0.24, Description = "0.0048 rounds down to 0.00")]
    public void SavingsAccount_Returns200AndStoresBalanceWithInterest(decimal balance, decimal expected)
    {
        var seeded = Store.SeedSavings(balance);

        var account = Api.AddInterest(seeded.Id).ShouldReturnAccount();

        Assert.Multiple(() =>
        {
            Assert.That(account, Is.EqualTo(new AccountJson(seeded.Id, "SAVINGS", expected)));
            Assert.That(Store.BalanceOf(seeded.Id), Is.EqualTo(expected));
        });
    }

    [Test]
    public void AddedTwice_Compounds()
    {
        var seeded = Store.SeedSavings(balance: 100m);

        Api.AddInterest(seeded.Id).StatusCode(200);
        var account = Api.AddInterest(seeded.Id).ShouldReturnAccount();

        Assert.That(account.Balance, Is.EqualTo(104.04m));
    }

    [TestCase(0)]
    [TestCase(1000)]
    [TestCase(-1000)]
    public void CheckingAccount_Returns422ProblemAndKeepsBalance(decimal balance)
    {
        var seeded = Store.SeedChecking(balance);

        Api.AddInterest(seeded.Id).ShouldReturnProblem(422, "Interest not allowed",
            $"Interest can only be added to savings accounts, but account {seeded.Id} is of type CHECKING.");

        Assert.That(Store.BalanceOf(seeded.Id), Is.EqualTo(balance));
    }

    [Test]
    public void UnknownAccount_Returns404Problem()
    {
        Api.AddInterest(UnknownAccountId)
            .ShouldReturnProblem(404, "Account not found", $"Account with id {UnknownAccountId} was not found.");
    }

    [Test]
    public void IdThatIsNotAnInteger_Returns404()
    {
        Api.AddInterest("abc").StatusCode(404);
    }
}
