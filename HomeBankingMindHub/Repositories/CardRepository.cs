using HomeBankingMindHub.Models;
using HomeBankingMindHub.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace HomeBankingMinHub.Repositories
{
    public class CardRepository : RepositoryBase<Card>, ICardRepository
    {
        public CardRepository(HomeBankingContext repositoryContext) : base(repositoryContext)
        {
        }
        public Card FindById(long id)
        {
            return RepositoryContext.Cards.Where(card => card.Id == id).FirstOrDefault();
        }
        public IEnumerable<Card> GetAllCards()
        {
            return FindAll()
                .ToList();
        }
        public IEnumerable<Card> GetCardsByClient(long clientId)
        {
            return FindByCondition(card => card.ClientId == clientId)
           .ToList();
        }
        public void Save(Card card)
        {
            Create(card);
            SaveChanges();
        }
        public string GenerateNextCardNumber()
        {
            var random = new Random();
            var cardNumber = new StringBuilder();

            for (int i = 0; i < 16; i++)
            {
                int digit = random.Next(0, 10);
                cardNumber.Append(digit);

                if ((i + 1) % 4 == 0 && i < 15)
                {
                    cardNumber.Append("-");
                }
            }
            return cardNumber.ToString();
        }

        public string GetLastCardNumber()
        {
            var lastCard = FindAll()
                .OrderByDescending(card => card.Id)
                .FirstOrDefault();
            if (lastCard == null)
            {
                return null;
            }
            return lastCard.Number;
        }
    }
}