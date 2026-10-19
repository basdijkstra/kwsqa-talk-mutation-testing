using BankAccounts.Api.Tests.Integration.Support;

namespace BankAccounts.Api.Tests.Integration.Endpoints;

/// <summary>End-to-end flows that only use the API, without seeding the store.</summary>
[TestFixture]
public class AccountJourneyTests : ApiTestBase
{
    [Test]
    public void SavingsAccount_FullLifecycle()
    {
        var id = Api.CreateAccount("SAVINGS").ShouldReturnAccount(201).Id;

        Api.Deposit(id, 1000m).ShouldReturnAccount();
        Api.AddInterest(id).ShouldReturnAccount();
        Api.Withdraw(id, 20m).ShouldReturnAccount();

        Assert.That(Api.GetAccount(id).ShouldReturnAccount(), Is.EqualTo(new AccountJson(id, "SAVINGS", 1000m)));

        Api.DeleteAccount(id).StatusCode(204);
        Api.GetAccount(id).StatusCode(404);
    }

    [Test]
    public void CheckingAccount_CanUseFullOverdraftAndRecover()
    {
        var id = Api.CreateAccount("CHECKING").ShouldReturnAccount(201).Id;

        Api.Withdraw(id, 2500m).ShouldReturnAccount();
        Api.Withdraw(id, 0.01m).StatusCode(422);
        Api.Deposit(id, 2500m).ShouldReturnAccount();

        Assert.That(Api.GetAccount(id).ShouldReturnAccount().Balance, Is.EqualTo(0m));
    }
}
