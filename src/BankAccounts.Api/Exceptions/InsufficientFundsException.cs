namespace BankAccounts.Api.Exceptions;

public class InsufficientFundsException(int accountId, decimal balance, decimal amount)
    : Exception(FormattableString.Invariant($"Withdrawal of {amount} from account {accountId} with balance {balance} exceeds the allowed overdraft."));
