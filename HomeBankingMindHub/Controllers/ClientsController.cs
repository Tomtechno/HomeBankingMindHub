using HomeBankingMindHub.Models;
using HomeBankingMindHub.Models.DTO;
using HomeBankingMindHub.Models.Enum;
using HomeBankingMindHub.Repositories.Interfaces;
using HomeBankingMinHub.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace HomeBankingMindHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase
    {

        private IClientRepository _clientRepository;
        private ICardRepository _cardRepository;
        private AccountsController _accountsController;
        private CardsController _cardsController;

        public ClientsController(IClientRepository clientRepository, ICardRepository cardRepository, AccountsController accountController, CardsController cardController)
        {
            _clientRepository = clientRepository;
            _cardRepository = cardRepository;
            _accountsController = accountController;
            _cardsController = cardController;

        }

        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                var clients = _clientRepository.GetAllClients();
                var clientsDTO = new List<ClientDTO>();
                foreach (Client client in clients)
                {
                    var newClientDTO = new ClientDTO
                    {
                        Id = client.Id,
                        Email = client.Email,
                        FirstName = client.FirstName,
                        LastName = client.LastName,
                        Accounts = client.Accounts.Select(ac => new AccountDTO
                        {
                            Id = ac.Id,
                            Balance = ac.Balance,
                            CreationDate = ac.CreationDate,
                            Number = ac.Number
                        }).ToList(),
                        Credits = client.Credits.Select(cl => new ClientLoanDTO
                        {
                            Id = cl.Id,
                            LoanId = cl.LoanId,
                            Name = cl.Loan.Name,
                            Amount = cl.Amount,
                            Payments = int.Parse(cl.Payments)
                        }).ToList(),
                        Cards = client.Cards.Select(c => new CardDTO
                        {
                            Id = c.Id,
                            CardHolder = c.CardHolder,
                            Color = c.Color,
                            Cvv = c.Cvv,
                            FromDate = c.FromDate,
                            Number = c.Number,
                            ThruDate = c.ThruDate,
                            Type = c.Type
                        }).ToList()
                    };
                    clientsDTO.Add(newClientDTO);
                }
                return Ok(clientsDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{id}")]
        public IActionResult Get(long id)
        {
            try
            {
                var client = _clientRepository.FindById(id);
                if (client == null)
                { return NotFound(); }
                var clientDTO = new ClientDTO
                {
                    Id = client.Id,
                    Email = client.Email,
                    FirstName = client.FirstName,
                    LastName = client.LastName,
                    Accounts = client.Accounts.Select(ac => new AccountDTO
                    {
                        Id = ac.Id,
                        Balance = ac.Balance,
                        CreationDate = ac.CreationDate,
                        Number = ac.Number
                    }).ToList(),
                    Credits = client.Credits.Select
                        (cl => new ClientLoanDTO
                        {
                            Id = cl.Id,
                            LoanId = cl.LoanId,
                            Name = cl.Loan.Name,
                            Amount = cl.Amount,
                            Payments = int.Parse(cl.Payments)
                        }
                        ).ToList(),
                    Cards = client.Cards.Select(c => new CardDTO
                    {
                        Id = c.Id,
                        CardHolder = c.CardHolder,
                        Color = c.Color,
                        Cvv = c.Cvv,
                        FromDate = c.FromDate,
                        Number = c.Number,
                        ThruDate = c.ThruDate,
                        Type = c.Type
                    }).ToList()
                };
                return Ok(clientDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("current")]
        public IActionResult GetCurrent()
        {
            try
            {
                string email = User.FindFirst("Client") != null ? User.FindFirst("Client").Value : string.Empty;
                if (email == string.Empty)
                {
                    return Forbid();
                }
                Client client = _clientRepository.FindByEmail(email);
                if (client == null)
                {
                    return Forbid();
                }
                var clientDTO = new ClientDTO
                {
                    Id = client.Id,
                    Email = client.Email,
                    FirstName = client.FirstName,
                    LastName = client.LastName,
                    Accounts = client.Accounts.Select(ac => new AccountDTO
                    {
                        Id = ac.Id,
                        Balance = ac.Balance,
                        CreationDate = ac.CreationDate,
                        Number = ac.Number
                    }).ToList(),
                    Credits = client.Credits.Select(cl => new ClientLoanDTO
                    {
                        Id = cl.Id,
                        LoanId = cl.LoanId,
                        Name = cl.Loan.Name,
                        Amount = cl.Amount,
                        Payments = int.Parse(cl.Payments)
                    }).ToList(),
                    Cards = client.Cards.Select(c => new CardDTO
                    {
                        Id = c.Id,
                        CardHolder = c.CardHolder,
                        Color = c.Color,
                        Cvv = c.Cvv,
                        FromDate = c.FromDate,
                        Number = c.Number,
                        ThruDate = c.ThruDate,
                        Type = c.Type
                    }).ToList()
                };
                return Ok(clientDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public IActionResult Post([FromBody] Client client)
        {
            try
            {
                if (client.FirstName.Length <= 2 || client.LastName.Length <= 2)
                {
                    return StatusCode(403, "El nombre y el apellido deben tener al menos tres caracteres");
                }
                if (!Regex.IsMatch(client.FirstName, @"^[a-zA-Z\s]+$"))
                {
                    return StatusCode(403, "El nombre no puede tener caracteres especiales");
                }
                if (!Regex.IsMatch(client.LastName, @"^[a-zA-Z\s]+$"))
                {
                    return StatusCode(403, "El apellido no puede tener caracteres especiales");
                }
                if (!(Regex.IsMatch(client.Email, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$")))
                {
                    return StatusCode(403, "El email es invalido");
                }
                if (client.Password.Length <= 7)
                {
                    return StatusCode(403, "La contrasenia debe tener al menos 8 caracteres");
                }
                if (!(Regex.IsMatch(client.Password, "[A-Z]") && Regex.IsMatch(client.Password, "[a-z]") && Regex.IsMatch(client.Password, @"\d")))
                {
                    return StatusCode(403, "La contrasenia debe contener al menos 1 mayuscula, 1 minuscula y 1 numero");
                }
                Client user = _clientRepository.FindByEmail(client.Email);
                if (user != null)
                {
                    return StatusCode(403, "El email está en uso");
                }
                Client newClient = new Client
                {
                    Email = client.Email,
                    Password = client.Password,
                    FirstName = client.FirstName,
                    LastName = client.LastName,
                };
                _clientRepository.Save(newClient);
                _accountsController.Post(newClient.Id);
                return Created("Creado con exito", newClient);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("current/accounts")]
        public IActionResult PostAccounts()
        {
            try
            {
                string email = User.FindFirst("Client") != null ? User.FindFirst("Client").Value : string.Empty;
                if (email == string.Empty)
                {
                    return Forbid();
                }
                Client client = _clientRepository.FindByEmail(email);
                if (client == null)
                {
                    return Forbid();
                }
                if (client.Accounts.Count >= 3)
                {
                    return StatusCode(403, "Ha alcanzado la cantidad maxima de cuentas, usted ya tiene 3 cuentas");
                }
                var account = _accountsController.Post(client.Id);
                if (account == null)
                {
                    return StatusCode(500, "Error al crear la cuenta");
                }
                return Created("Creado con exito", account);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("current/accounts")]
        public IActionResult GetAccounts()
        {
            try
            {
                string email = User.FindFirst("Client")?.Value;
                if (string.IsNullOrEmpty(email))
                {
                    return Forbid();
                }
                Client client = _clientRepository.FindByEmail(email);
                if (client == null)
                {
                    return Forbid();
                }
                var accounts = client.Accounts.Select(ac => new AccountDTO
                {
                    Id = ac.Id,
                    Balance = ac.Balance,
                    CreationDate = ac.CreationDate,
                    Number = ac.Number
                }).ToList();
                return Ok(accounts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("current/cards")]
        public IActionResult PostCards([FromBody] Card card)
        {
            try
            {
                string email = User.FindFirst("Client") != null ? User.FindFirst("Client").Value : string.Empty;
                if (email == string.Empty)
                {
                    return Forbid();
                }
                Client client = _clientRepository.FindByEmail(email);
                var digits1 = new Random().Next(1000, 9999);
                var digits2 = new Random().Next(1000, 9999);
                var digits3 = new Random().Next(1000, 9999);
                var digits4 = new Random().Next(1000, 9999);
                string newCardNumber;
                do
                {
                    newCardNumber = digits1.ToString() + "-" + digits2.ToString() + "-" + digits3.ToString() + "-" + digits4.ToString();
                }
                while (_cardRepository.FindById(newCardNumber) != null);
                Card newCard = new Card
                {
                    CardHolder = client.FirstName + " " + client.LastName,
                    Type = card.Type,
                    Color = card.Color,
                    Number = newCardNumber,
                    Cvv = new Random().Next(100, 999),
                    FromDate = DateTime.Now,
                    ThruDate = DateTime.Now.AddYears(4),
                    ClientId = client.Id
                };
                if (card.Type != CardType.CREDIT.ToString() && card.Type != CardType.DEBIT.ToString())
                {
                    return StatusCode(400, "El tipo de tarjeta es invalido");
                }
                if (card.Color != CardColor.GOLD.ToString() && card.Color != CardColor.SILVER.ToString() && card.Color != CardColor.TITANIUM.ToString())
                {
                    return StatusCode(400, "El color de tarjeta es invalido");
                }
                int CardCount = client.Cards.Where(c => c.Type == card.Type).Count();
                if (CardCount >= 3)
                {
                    return StatusCode(403, "Ha alcanzado la cantidad limite de tarjetas, ya tiene 3 tarjetas del mismo tipo");
                }






                int sameCard = client.Cards.Where(c => c.Color == card.Color && c.Type == card.Type).Count();
                if (sameCard == 1)
                {
                    return StatusCode(403, "Ya tiene una tarjeta del mismo tipo y color");
                }
                var newCardDto = _cardsController.Post(newCard);
                if (newCard == null)
                {
                    return StatusCode(500, "Error al crear la tarjeta");
                }
                return Created("Creado con exito", newCardDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("current/cards")]
        public IActionResult GetCards()
        {
            try
            {
                string email = User.FindFirst("Client")?.Value;
                if (string.IsNullOrEmpty(email))
                {
                    return Forbid();
                }
                Client client = _clientRepository.FindByEmail(email);
                if (client == null)
                {
                    return Forbid();
                }
                var cards = client.Cards.Select(car => new CardDTO
                {
                    Id = car.Id,
                    CardHolder = car.CardHolder,
                    Color = car.Color,
                    Cvv = car.Cvv,
                    FromDate = car.FromDate,
                    Number = car.Number,
                    ThruDate = car.ThruDate,
                    Type = car.Type
                }).ToList();
                return Ok(cards);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

    }
}