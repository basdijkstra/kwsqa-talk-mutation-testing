using System.ComponentModel.DataAnnotations;

namespace BankAccounts.Api.Models;

public record CreateAccountRequest([Required] AccountType? Type);

public record TransactionRequest([Required] decimal? Amount);

public record AccountResponse(int Id, AccountType Type, decimal Balance)
{
    public static AccountResponse From(Account account) =>
        new(account.Id, account.Type, account.Balance);
}
