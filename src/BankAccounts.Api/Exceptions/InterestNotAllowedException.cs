using BankAccounts.Api.Models;

namespace BankAccounts.Api.Exceptions;

public class InterestNotAllowedException(int accountId, AccountType type)
    : Exception($"Interest can only be added to savings accounts, but account {accountId} is of type {type.ToString().ToUpperInvariant()}.");
