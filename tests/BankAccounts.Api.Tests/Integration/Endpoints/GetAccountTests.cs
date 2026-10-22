using BankAccounts.Api.Tests.Integration.Support;

namespace BankAccounts.Api.Tests.Integration.Endpoints;

[TestFixture]
public class GetAccountTests : ApiTestBase
{
    [Test]
    public void ReturnsTheRequestedAccountWhenThereAreSeveral()
    {
        Store.SeedChecking(balance: 1m);
        var wanted = Store.SeedSavings(balance: 2m);
        Store.SeedChecking(balance: 3m);

        var account = Api.GetAccount(wanted.Id).ShouldReturnAccount();

        Assert.That(account, Is.EqualTo(new AccountJson(wanted.Id, "SAVINGS", 2m)));
    }

    [TestCase("abc")]
    public void IdThatIsNotAnInteger_Returns404(string id)
    {
        Api.GetAccount(id).StatusCode(404);
    }
}
