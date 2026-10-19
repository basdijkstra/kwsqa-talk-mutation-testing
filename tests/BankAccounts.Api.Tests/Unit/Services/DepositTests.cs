namespace BankAccounts.Api.Tests.Unit.Services;

[TestFixture]
public class DepositTests : AccountServiceTestBase
{
    [Test]
    public void Deposit_IntoOverdrawnCheckingAccount_ReducesOverdraft()
    {
        var account = GivenCheckingAccount(balance: -2500m);

        Service.Deposit(account.Id, 500m);

        Assert.That(StoredBalanceOf(account), Is.EqualTo(-2000m));
    }
}
