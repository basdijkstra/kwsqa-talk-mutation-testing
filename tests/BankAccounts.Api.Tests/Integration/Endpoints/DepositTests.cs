using BankAccounts.Api.Tests.Integration.Support;

namespace BankAccounts.Api.Tests.Integration.Endpoints;

[TestFixture]
public class DepositTests : ApiTestBase
{
    [TestCase(0.01, Description = "Smallest positive amount in cents")]
    [TestCase(1)]
    [TestCase(999_999.99)]
    public void PositiveAmount_Returns200AndStoresNewBalance(decimal amount)
    {
        var seeded = Store.SeedSavings(balance: 100m);

        var account = Api.Deposit(seeded.Id, amount).ShouldReturnAccount();

        Assert.Multiple(() =>
        {
            Assert.That(account, Is.EqualTo(new AccountJson(seeded.Id, "SAVINGS", 100m + amount)));
            Assert.That(Store.BalanceOf(seeded.Id), Is.EqualTo(100m + amount));
        });
    }

    [Test]
    public void IntoOverdrawnCheckingAccount_ReducesOverdraft()
    {
        var seeded = Store.SeedChecking(balance: -2500m);

        var account = Api.Deposit(seeded.Id, 2500.01m).ShouldReturnAccount();

        Assert.That(account, Is.EqualTo(new AccountJson(seeded.Id, "CHECKING", 0.01m)));
    }

    [Test]
    public void SeveralDeposits_AddUp()
    {
        var seeded = Store.SeedChecking();

        Api.Deposit(seeded.Id, 10m).StatusCode(200);
        Api.Deposit(seeded.Id, 0.55m).StatusCode(200);

        Assert.That(Store.BalanceOf(seeded.Id), Is.EqualTo(10.55m));
    }

    [Test]
    public void OnlyTheTargetAccountChanges()
    {
        var target = Store.SeedChecking(balance: 1m);
        var other = Store.SeedChecking(balance: 1m);

        Api.Deposit(target.Id, 10m).StatusCode(200);

        Assert.That(Store.BalanceOf(other.Id), Is.EqualTo(1m));
    }

    [TestCase(0, "0")]
    [TestCase(-0.01, "-0.01")]
    [TestCase(-100, "-100")]
    public void ZeroOrNegativeAmount_Returns400ProblemAndKeepsBalance(decimal amount, string amountInMessage)
    {
        var seeded = Store.SeedSavings(balance: 100m);

        Api.DepositWithJson(seeded.Id, $$"""{"amount": {{amountInMessage}}}""")
            .ShouldReturnProblem(400, "Invalid amount", $"Amount must be greater than 0, but was {amountInMessage}.");

        Assert.That(Store.BalanceOf(seeded.Id), Is.EqualTo(100m));
    }

    [Test]
    public void UnknownAccount_Returns404Problem()
    {
        Api.Deposit(UnknownAccountId, 10m)
            .ShouldReturnProblem(404, "Account not found", $"Account with id {UnknownAccountId} was not found.");
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
    [TestCase("""{"amount": "ten"}""", Description = "Amount as non-numeric string")]
    [TestCase("""{"amount": ""}""", Description = "Amount as empty string")]
    [TestCase("""{"amount": true}""", Description = "Amount as boolean")]
    [TestCase("""{"amount": 1e30}""", Description = "Larger than decimal.MaxValue")]
    [TestCase("""{"amount": 10""", Description = "Malformed JSON")]
    [TestCase("", Description = "Empty body")]
    public void InvalidBody_Returns400AndKeepsBalance(string json)
    {
        var seeded = Store.SeedSavings(balance: 100m);

        Api.DepositWithJson(seeded.Id, json).ShouldReturnValidationProblem();

        Assert.That(Store.BalanceOf(seeded.Id), Is.EqualTo(100m));
    }

    [Test]
    public void IdThatIsNotAnInteger_Returns404()
    {
        Api.Deposit("abc", 10m).StatusCode(404);
    }
}
