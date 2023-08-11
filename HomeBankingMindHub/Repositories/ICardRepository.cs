using HomeBankingMindHub.Models;
using System.Collections;
using System.Collections.Generic;

namespace HomeBankingMindHub.Repositories
{
    public interface ICardRepository
    {
        void Save(Card card);
        IEnumerable<Card> GetAllCards();
        Card FindById(long id);
        IEnumerable<Card> GetCardsByClient(long clientId);
    }
}
