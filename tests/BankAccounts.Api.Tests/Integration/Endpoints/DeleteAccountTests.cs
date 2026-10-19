using BankAccounts.Api.Tests.Integration.Support;

namespace BankAccounts.Api.Tests.Integration.Endpoints;

[TestFixture]
public class DeleteAccountTests : ApiTestBase
{
    [Test]
    public void ExistingAccount_Returns204AndRemovesItFromStore()
    {
        var account = Store.SeedSavings(balance: 100m);

        Api.DeleteAccount(account.Id).ShouldReturnEmptyResponse(204);

        Assert.That(Store.Find(account.Id), Is.Null);
    }

    [Test]
    public void DeletedAccount_CanNoLongerBeRetrieved()
    {
        var account = Store.SeedChecking();

        Api.DeleteAccount(account.Id).StatusCode(204);

        Api.GetAccount(account.Id).ShouldReturnProblem(404, "Account not found", $"Account with id {account.Id} was not found.");
    }

    [Test]
    public void OnlyTheRequestedAccountIsDeleted()
    {
        var toDelete = Store.SeedChecking();
        var toKeep = Store.SeedSavings(balance: 5m);

        Api.DeleteAccount(toDelete.Id).StatusCode(204);

        Assert.That(Store.Find(toKeep.Id), Is.Not.Null);
    }

    [Test]
    public void AccountWithNegativeBalance_CanBeDeleted()
    {
        var account = Store.SeedChecking(balance: -2500m);

        Api.DeleteAccount(account.Id).StatusCode(204);

        Assert.That(Store.Find(account.Id), Is.Null);
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(UnknownAccountId)]
    public void UnknownId_Returns404Problem(int id)
    {
        Api.DeleteAccount(id).ShouldReturnProblem(404, "Account not found", $"Account with id {id} was not found.");
    }

    [Test]
    public void DeletingTwice_SecondTimeReturns404()
    {
        var account = Store.SeedChecking();
        Api.DeleteAccount(account.Id).StatusCode(204);

        Api.DeleteAccount(account.Id).ShouldReturnProblem(404, "Account not found", $"Account with id {account.Id} was not found.");
    }

    [Test]
    public void IdThatIsNotAnInteger_Returns404()
    {
        Api.DeleteAccount("abc").StatusCode(404);
    }
}
