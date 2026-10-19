using BankAccounts.Api.Models;

namespace BankAccounts.Api.Services;

public interface IAccountService
{
    Account GetAccount(int id);

    Account CreateAccount(AccountType type);

    void DeleteAccount(int id);

    Account Deposit(int id, decimal amount);

    Account Withdraw(int id, decimal amount);

    Account AddInterest(int id);
}
