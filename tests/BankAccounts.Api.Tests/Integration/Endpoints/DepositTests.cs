using BankAccounts.Api.Tests.Integration.Support;

namespace BankAccounts.Api.Tests.Integration.Endpoints;

[TestFixture]
public class DepositTests : ApiTestBase
{
    [Test]
    public void OnlyTheTargetAccountChanges()
    {
        var target = Store.SeedChecking(balance: 1m);
        var other = Store.SeedChecking(balance: 1m);

        Api.Deposit(target.Id, 10m).StatusCode(200);

        Assert.That(Store.BalanceOf(other.Id), Is.EqualTo(1m));
    }

    [Test]
    public void InvalidAmountForUnknownAccount_Returns400()
    {
        Api.DepositWithJson(UnknownAccountId, """{"amount": 0}""")
            .ShouldReturnProblem(400, "Invalid amount", "Amount must be greater than 0, but was 0.");
    }

    // ASP.NET's web JSON defaults allow numbers inside strings.
    [Test]
    public void AmountAsNumericString_IsAccepted()
    {
        var seeded = Store.SeedSavings(balance: 100m);

        var account = Api.DepositWithJson(seeded.Id, """{"amount": "10.50"}""").ShouldReturnAccount();

        Assert.That(account.Balance, Is.EqualTo(110.50m));
    }

    [TestCase("""{}""", Description = "Amount missing")]
    [TestCase("""{"amount": null}""", Description = "Amount null")]
    public void InvalidBody_Returns400AndKeepsBalance(string json)
    {
        var seeded = Store.SeedSavings(balance: 100m);

        Api.DepositWithJson(seeded.Id, json).ShouldReturnValidationProblem();

        Assert.That(Store.BalanceOf(seeded.Id), Is.EqualTo(100m));
    }
}
