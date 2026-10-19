using RestAssured.Request;
using RestAssured.Request.Builders;
using RestAssured.Response;
using static RestAssured.Dsl;

namespace BankAccounts.Api.Tests.Integration.Support;

/// <summary>
/// One method per endpoint, so tests read as "what is called" instead of "how HTTP works".
/// Every method returns the RestAssured response, ready for Then()-style checks.
/// The *WithJson variants send a raw body, for testing malformed or unexpected input.
/// </summary>
public class AccountsApi(HttpClient client)
{
    private const string Accounts = "/accounts";

    // RestAssured needs the base URI in a spec to resolve relative paths.
    private readonly RequestSpecification _spec = new RequestSpecBuilder()
        .WithBaseUri(client.BaseAddress!.ToString())
        .Build();

    public VerifiableResponse GetAccount(object id) =>
        Request()
            .When()
            .Get($"{Accounts}/{id}")
            .Then();

    public VerifiableResponse GetLocation(string location) =>
        Given(client)
            .When()
            .Get(location)
            .Then();

    public VerifiableResponse CreateAccount(string? type) =>
        Request()
            .Body(new { type })
            .When()
            .Post(Accounts)
            .Then();

    public VerifiableResponse CreateAccountWithJson(string json, string contentType = "application/json") =>
        Request()
            .ContentType(contentType)
            .Body(json)
            .When()
            .Post(Accounts)
            .Then();

    public VerifiableResponse DeleteAccount(object id) =>
        Request()
            .When()
            .Delete($"{Accounts}/{id}")
            .Then();

    public VerifiableResponse Deposit(object id, decimal amount) =>
        Transaction(id, "deposits", new { amount });

    public VerifiableResponse DepositWithJson(object id, string json) =>
        TransactionWithJson(id, "deposits", json);

    public VerifiableResponse Withdraw(object id, decimal amount) =>
        Transaction(id, "withdrawals", new { amount });

    public VerifiableResponse WithdrawWithJson(object id, string json) =>
        TransactionWithJson(id, "withdrawals", json);

    public VerifiableResponse AddInterest(object id) =>
        Request()
            .When()
            .Post($"{Accounts}/{id}/interest")
            .Then();

    private ExecutableRequest Request() => Given(client).Spec(_spec);

    private VerifiableResponse Transaction(object id, string kind, object body) =>
        Request()
            .Body(body)
            .When()
            .Post($"{Accounts}/{id}/{kind}")
            .Then();

    private VerifiableResponse TransactionWithJson(object id, string kind, string json) =>
        Request()
            .ContentType("application/json")
            .Body(json)
            .When()
            .Post($"{Accounts}/{id}/{kind}")
            .Then();
}
