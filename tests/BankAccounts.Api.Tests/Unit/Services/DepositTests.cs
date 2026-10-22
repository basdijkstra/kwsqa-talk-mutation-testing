using BankAccounts.Api.Exceptions;

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

    [TestCase(0, "0")]
    public void Deposit_ZeroOrNegativeAmount_ThrowsInvalidAmount(decimal amount, string amountInMessage)
    {
        var account = GivenCheckingAccount(balance: 100m);

        var ex = Assert.Throws<InvalidAmountException>(() => Service.Deposit(account.Id, amount));

        Assert.Multiple(() =>
        {
            Assert.That(ex!.Message, Is.EqualTo($"Amount must be greater than 0, but was {amountInMessage}."));
            Assert.That(StoredBalanceOf(account), Is.EqualTo(100m));
            Assert.That(Repository.UpdateCount, Is.Zero);
        });
    }
}
