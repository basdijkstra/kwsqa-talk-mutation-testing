using BankAccounts.Api.Tests.Integration.Support;

namespace BankAccounts.Api.Tests.Integration.Endpoints;

[TestFixture]
public class DepositTests : ApiTestBase
{
    [Test]
    public void InvalidAmountForUnknownAccount_Returns400()
    {
        Api.DepositWithJson(UnknownAccountId, """{"amount": 0}""")
            .ShouldReturnProblem(400, "Invalid amount", "Amount must be greater than 0, but was 0.");
    }
}
