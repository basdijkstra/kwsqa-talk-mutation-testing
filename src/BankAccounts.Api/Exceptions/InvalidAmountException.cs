namespace BankAccounts.Api.Exceptions;

public class InvalidAmountException(decimal amount)
    : Exception(FormattableString.Invariant($"Amount must be greater than 0, but was {amount}."));
