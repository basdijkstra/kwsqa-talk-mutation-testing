using BankAccounts.Api.Tests.Integration.Support;

namespace BankAccounts.Api.Tests.Integration.Endpoints;

[TestFixture]
public class AddInterestTests : ApiTestBase
{
    [TestCase(0)]
    public void CheckingAccount_Returns422ProblemAndKeepsBalance(decimal balance)
    {
        var seeded = Store.SeedChecking(balance);

        Api.AddInterest(seeded.Id).ShouldReturnProblem(422, "Interest not allowed",
            $"Interest can only be added to savings accounts, but account {seeded.Id} is of type CHECKING.");

        Assert.That(Store.BalanceOf(seeded.Id), Is.EqualTo(balance));
    }
}
