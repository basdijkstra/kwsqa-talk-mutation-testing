using BankAccounts.Api.Tests.Integration.Support;

namespace BankAccounts.Api.Tests.Integration.Endpoints;

[TestFixture]
public class CreateAccountTests : ApiTestBase
{
    [TestCase("""{"type": 0}""", Description = "Numeric enum values are rejected")]
    public void InvalidBody_Returns400AndCreatesNothing(string json)
    {
        Api.CreateAccountWithJson(json).ShouldReturnValidationProblem();

        Assert.That(Store.Find(1), Is.Null);
    }
}
