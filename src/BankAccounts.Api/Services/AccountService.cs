using BankAccounts.Api.Exceptions;
using BankAccounts.Api.Models;
using BankAccounts.Api.Repositories;

namespace BankAccounts.Api.Services;

public class AccountService(IAccountRepository repository) : IAccountService
{
    public const decimal CheckingOverdraftLimit = 2500m;
    public const decimal InterestRate = 0.02m;

    public Account GetAccount(int id) =>
        repository.GetById(id) ?? throw new AccountNotFoundException(id);

    public Account CreateAccount(AccountType type) =>
        repository.Add(new Account { Type = type, Balance = 0m });

    public void DeleteAccount(int id)
    {
        if (!repository.Delete(id))
        {
            throw new AccountNotFoundException(id);
        }
    }

    public Account Deposit(int id, decimal amount)
    {
        EnsurePositive(amount);

        var account = GetAccount(id);
        account.Balance += amount;
        repository.Update(account);

        return account;
    }

    public Account Withdraw(int id, decimal amount)
    {
        EnsurePositive(amount);

        var account = GetAccount(id);
        var newBalance = account.Balance - amount;

        var lowestAllowedBalance = account.Type switch
        {
            AccountType.Savings => 0m,
            AccountType.Checking => -CheckingOverdraftLimit,
            _ => throw new ArgumentOutOfRangeException(nameof(account.Type), account.Type, null)
        };

        if (newBalance < lowestAllowedBalance)
        {
            throw new InsufficientFundsException(id, account.Balance, amount);
        }

        account.Balance = newBalance;
        repository.Update(account);

        return account;
    }

    public Account AddInterest(int id)
    {
        var account = GetAccount(id);

        if (account.Type != AccountType.Savings)
        {
            throw new InterestNotAllowedException(id, account.Type);
        }

        account.Balance += Math.Round(account.Balance * InterestRate, 2);
        repository.Update(account);

        return account;
    }

    private static void EnsurePositive(decimal amount)
    {
        if (amount <= 0)
        {
            throw new InvalidAmountException(amount);
        }
    }
}
