using System.Globalization;
using BankAccounts.Api.Tests.Integration.Support;

namespace BankAccounts.Api.Tests.Integration.Endpoints;

[TestFixture]
public class WithdrawTests : ApiTestBase
{
    // Checking accounts may go down to -2500. Savings accounts may not go below 0.

    [TestCase(0, 2500.01, Description = "One cent past the overdraft limit")]
    public void Checking_BeyondOverdraftLimit_Returns422ProblemAndKeepsBalance(decimal balance, decimal amount)
    {
        var seeded = Store.SeedChecking(balance);

        Api.Withdraw(seeded.Id, amount)
            .ShouldReturnProblem(422, "Insufficient funds", InsufficientFundsMessage(seeded.Id, balance, amount));

        Assert.That(Store.BalanceOf(seeded.Id), Is.EqualTo(balance));
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
