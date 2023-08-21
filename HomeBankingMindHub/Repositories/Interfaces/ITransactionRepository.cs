using HomeBankingMindHub.Models;

namespace HomeBankingMindHub.Repositories.Interfaces
{
    public interface ITransactionRepository
    {
        void Save(Transaction transaction);

        Transaction FindByNumber(long id);
    }
}