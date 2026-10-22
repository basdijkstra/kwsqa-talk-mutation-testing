using BankAccounts.Api.Tests.Integration.Support;

namespace BankAccounts.Api.Tests.Integration.Endpoints;

[TestFixture]
public class CreateAccountTests : ApiTestBase
{
    [Test]
    public void LocationHeader_PointsToTheNewAccount()
    {
        var response = Api.CreateAccount("SAVINGS").StatusCode(201);
        var location = response.Extract().Header("Location");

        Assert.That(location, Does.EndWith("/accounts/1"));

        var account = Api.GetLocation(location).ShouldReturnAccount();
        Assert.That(account, Is.EqualTo(new AccountJson(1, "SAVINGS", 0m)));
    }

    [Test]
    public void TypeIsNull_Returns400()
    {
        Api.CreateAccount(null).ShouldReturnValidationProblem();
    }

    [TestCase("""{}""", Description = "Type missing")]
    [TestCase("""{"type": 0}""", Description = "Numeric enum values are rejected")]
    public void InvalidBody_Returns400AndCreatesNothing(string json)
    {
        Api.CreateAccountWithJson(json).ShouldReturnValidationProblem();

        Assert.That(Store.Find(1), Is.Null);
    }

    [Test]
    public void BodyThatIsNotJson_Returns415()
    {
        Api.CreateAccountWithJson("type=CHECKING", contentType: "text/plain").StatusCode(415);
    }
}
