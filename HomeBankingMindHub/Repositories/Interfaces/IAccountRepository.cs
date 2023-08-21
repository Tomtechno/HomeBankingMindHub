using HomeBankingMindHub.Models;
using System.Collections.Generic;

namespace HomeBankingMindHub.Repositories.Interfaces
{
    public interface IAccountRepository
    {
        IEnumerable<Account> GetAllAccounts();

        void Save(Account account);

        Account FindById(long id);

        IEnumerable<Account> GetAccountsByClient(long clientId);

        Account FindByNumber(string number);
    }
}
