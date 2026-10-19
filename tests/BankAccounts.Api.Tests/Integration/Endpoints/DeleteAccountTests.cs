using BankAccounts.Api.Tests.Integration.Support;

namespace BankAccounts.Api.Tests.Integration.Endpoints;

[TestFixture]
public class DeleteAccountTests : ApiTestBase
{
    [Test]
    public void DeletingTwice_SecondTimeReturns404()
    {
        var account = Store.SeedChecking();
        Api.DeleteAccount(account.Id).StatusCode(204);

        Api.DeleteAccount(account.Id).ShouldReturnProblem(404, "Account not found", $"Account with id {account.Id} was not found.");
    }
}
