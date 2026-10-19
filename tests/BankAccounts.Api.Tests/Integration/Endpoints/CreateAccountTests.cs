using BankAccounts.Api.Models;
using BankAccounts.Api.Tests.Integration.Support;

namespace BankAccounts.Api.Tests.Integration.Endpoints;

[TestFixture]
public class CreateAccountTests : ApiTestBase
{
    [TestCase("CHECKING", AccountType.Checking)]
    [TestCase("SAVINGS", AccountType.Savings)]
    public void ValidType_Returns201AndStoresEmptyAccount(string type, AccountType expectedType)
    {
        var account = Api.CreateAccount(type).ShouldReturnAccount(expectedStatus: 201);

        Assert.That(account, Is.EqualTo(new AccountJson(1, type, 0m)));

        var stored = Store.Find(account.Id);
        Assert.Multiple(() =>
        {
            Assert.That(stored, Is.Not.Null);
            Assert.That(stored!.Type, Is.EqualTo(expectedType));
            Assert.That(stored.Balance, Is.EqualTo(0m));
        });
    }

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
    public void SeveralAccounts_GetIncreasingIds()
    {
        var first = Api.CreateAccount("CHECKING").ShouldReturnAccount(201);
        var second = Api.CreateAccount("SAVINGS").ShouldReturnAccount(201);

        Assert.Multiple(() =>
        {
            Assert.That(first.Id, Is.EqualTo(1));
            Assert.That(second.Id, Is.EqualTo(2));
        });
    }

    [Test]
    public void TypeIsNull_Returns400()
    {
        Api.CreateAccount(null).ShouldReturnValidationProblem();
    }

    [TestCase("""{}""", Description = "Type missing")]
    [TestCase("""{"type": "INVESTMENT"}""", Description = "Unknown type")]
    [TestCase("""{"type": ""}""", Description = "Empty type")]
    [TestCase("""{"type": 0}""", Description = "Numeric enum values are rejected")]
    [TestCase("""{"type": 1}""", Description = "Numeric enum values are rejected")]
    [TestCase("""{"type": "CHECKING" """, Description = "Malformed JSON")]
    [TestCase("""[]""", Description = "Array instead of object")]
    [TestCase("", Description = "Empty body")]
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
