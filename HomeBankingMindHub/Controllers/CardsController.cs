using HomeBankingMindHub.Models;
using HomeBankingMindHub.Models.DTO;
using HomeBankingMindHub.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Drawing;

namespace HomeBankingMindHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]   
    public class CardsController : ControllerBase {
        private ICardRepository _cardRepository;
        public CardsController(ICardRepository cardRepository)
        {
            _cardRepository = cardRepository;
        }
        public IActionResult Post(Card newCard)
        {
            try
            {
                _cardRepository.Save(newCard);
                CardDTO newcardDTO = new CardDTO
                {
                    CardHolder = newCard.CardHolder,
                    Type = newCard.Type,
                    Color = newCard.Color,
                    Number = newCard.Number,
                    Cvv = newCard.Cvv,
                    FromDate = newCard.FromDate,
                    ThruDate = newCard.ThruDate,
                };
                return Created("", newcardDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
