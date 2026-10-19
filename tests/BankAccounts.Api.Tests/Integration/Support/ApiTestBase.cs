namespace BankAccounts.Api.Tests.Integration.Support;

/// <summary>
/// Starts a fresh API instance per test, so every test starts with an empty store
/// and account ids start at 1.
/// </summary>
public abstract class ApiTestBase
{
    protected const int UnknownAccountId = 999;

    private BankAccountsApiFactory _factory = null!;
    private HttpClient _client = null!;

    protected AccountsApi Api { get; private set; } = null!;

    protected AccountStore Store { get; private set; } = null!;

    [SetUp]
    public void StartApi()
    {
        _factory = new BankAccountsApiFactory();
        _client = _factory.CreateClient();
        Api = new AccountsApi(_client);
        Store = new AccountStore(_factory.Repository);
    }

    [TearDown]
    public void StopApi()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}
