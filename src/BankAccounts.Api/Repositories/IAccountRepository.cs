using BankAccounts.Api.Models;

namespace BankAccounts.Api.Repositories;

public interface IAccountRepository
{
    Account? GetById(int id);

    Account Add(Account account);

    void Update(Account account);

    bool Delete(int id);
}
