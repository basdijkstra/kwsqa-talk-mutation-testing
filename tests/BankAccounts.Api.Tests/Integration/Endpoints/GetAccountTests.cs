using BankAccounts.Api.Tests.Integration.Support;

namespace BankAccounts.Api.Tests.Integration.Endpoints;

[TestFixture]
public class GetAccountTests : ApiTestBase
{
    [Test]
    public void ExistingCheckingAccount_Returns200WithAccount()
    {
        var seeded = Store.SeedChecking(balance: -2500m);

        var account = Api.GetAccount(seeded.Id).ShouldReturnAccount();

        Assert.That(account, Is.EqualTo(new AccountJson(seeded.Id, "CHECKING", -2500m)));
    }

    [Test]
    public void ExistingSavingsAccount_Returns200WithAccount()
    {
        var seeded = Store.SeedSavings(balance: 1234.56m);

        var account = Api.GetAccount(seeded.Id).ShouldReturnAccount();

        Assert.That(account, Is.EqualTo(new AccountJson(seeded.Id, "SAVINGS", 1234.56m)));
    }

    [Test]
    public void ReturnsTheRequestedAccountWhenThereAreSeveral()
    {
        Store.SeedChecking(balance: 1m);
        var wanted = Store.SeedSavings(balance: 2m);
        Store.SeedChecking(balance: 3m);

        var account = Api.GetAccount(wanted.Id).ShouldReturnAccount();

        Assert.That(account, Is.EqualTo(new AccountJson(wanted.Id, "SAVINGS", 2m)));
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(2, Description = "Just above the highest existing id")]
    [TestCase(int.MaxValue)]
    public void UnknownId_Returns404Problem(int id)
    {
        Store.SeedChecking();

        Api.GetAccount(id).ShouldReturnProblem(404, "Account not found", $"Account with id {id} was not found.");
    }

    [TestCase("abc")]
    [TestCase("1.5")]
    [TestCase("2147483648", Description = "int.MaxValue + 1 does not match the route")]
    public void IdThatIsNotAnInteger_Returns404(string id)
    {
        Api.GetAccount(id).StatusCode(404);
    }
}
