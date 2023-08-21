using HomeBankingMindHub.Models;
using System.Collections;
using System.Collections.Generic;

namespace HomeBankingMindHub.Repositories.Interfaces
{
    public interface ICardRepository
    {
        void Save(Card card);

        IEnumerable<Card> GetAllCards();

        Card FindById(string number);
    }
}
