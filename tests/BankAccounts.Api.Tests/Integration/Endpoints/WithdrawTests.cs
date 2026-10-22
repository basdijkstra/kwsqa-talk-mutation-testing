using System.Globalization;
using BankAccounts.Api.Tests.Integration.Support;

namespace BankAccounts.Api.Tests.Integration.Endpoints;

[TestFixture]
public class WithdrawTests : ApiTestBase
{
    // Checking accounts may go down to -2500. Savings accounts may not go below 0.

    [TestCase(0, 2500, Description = "Lands exactly on the overdraft limit")]
    public void Checking_WithinOverdraftLimit_Returns200AndStoresNewBalance(decimal balance, decimal amount)
    {
        var seeded = Store.SeedChecking(balance);

        var account = Api.Withdraw(seeded.Id, amount).ShouldReturnAccount();

        Assert.Multiple(() =>
        {
            Assert.That(account, Is.EqualTo(new AccountJson(seeded.Id, "CHECKING", balance - amount)));
            Assert.That(Store.BalanceOf(seeded.Id), Is.EqualTo(balance - amount));
        });
    }

    [TestCase(0, 2500.01, Description = "One cent past the overdraft limit")]
    public void Checking_BeyondOverdraftLimit_Returns422ProblemAndKeepsBalance(decimal balance, decimal amount)
    {
        var seeded = Store.SeedChecking(balance);

        Api.Withdraw(seeded.Id, amount)
            .ShouldReturnProblem(422, "Insufficient funds", InsufficientFundsMessage(seeded.Id, balance, amount));

        Assert.That(Store.BalanceOf(seeded.Id), Is.EqualTo(balance));
    }

    [TestCase(100, 100.01, Description = "One cent more than the balance")]
    [TestCase(0, 0.01, Description = "Empty account")]
    public void Savings_BeyondBalance_Returns422ProblemAndKeepsBalance(decimal balance, decimal amount)
    {
        var seeded = Store.SeedSavings(balance);

        Api.Withdraw(seeded.Id, amount)
            .ShouldReturnProblem(422, "Insufficient funds", InsufficientFundsMessage(seeded.Id, balance, amount));

        Assert.That(Store.BalanceOf(seeded.Id), Is.EqualTo(balance));
    }

    [Test]
    public void FailedWithdrawal_DoesNotAffectLaterWithdrawals()
    {
        var seeded = Store.SeedSavings(balance: 50m);

        Api.Withdraw(seeded.Id, 50.01m).StatusCode(422);
        var account = Api.Withdraw(seeded.Id, 50m).ShouldReturnAccount();

        Assert.That(account.Balance, Is.EqualTo(0m));
    }

    [Test]
    public void InvalidAmountForUnknownAccount_Returns400()
    {
        Api.WithdrawWithJson(UnknownAccountId, """{"amount": -1}""")
            .ShouldReturnProblem(400, "Invalid amount", "Amount must be greater than 0, but was -1.");
    }

    private static string InsufficientFundsMessage(int id, decimal balance, decimal amount) =>
        string.Create(CultureInfo.InvariantCulture,
            $"Withdrawal of {amount} from account {id} with balance {balance} exceeds the allowed overdraft.");
}
