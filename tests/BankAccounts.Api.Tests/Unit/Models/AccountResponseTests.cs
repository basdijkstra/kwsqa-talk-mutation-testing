using BankAccounts.Api.Models;

namespace BankAccounts.Api.Tests.Unit.Models;

[TestFixture]
public class AccountResponseTests
{
    [TestCase(1, AccountType.Checking, -2500)]
    [TestCase(42, AccountType.Savings, 1234.56)]
    public void From_CopiesAllFields(int id, AccountType type, decimal balance)
    {
        var account = new Account { Id = id, Type = type, Balance = balance };

        var response = AccountResponse.From(account);

        Assert.That(response, Is.EqualTo(new AccountResponse(id, type, balance)));
    }
}
