using HomeBankingMindHub.dtos;
using HomeBankingMindHub.Models;
using HomeBankingMindHub.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        private AccountsController _accountsController;
        private CardsController _cardsController;
        public ClientsController(IClientRepository clientRepository, AccountsController accountController, CardsController cardController)
        {
            _clientRepository = clientRepository;
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
                var accounts = client.Accounts;
                return Ok(accounts);
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
                    return StatusCode(400, "name and last name should have more than 3 letters");
                }
                if (!Regex.IsMatch(client.FirstName, @"^[a-zA-Z\s]+$"))
                {
                    return StatusCode(400, "name can't have special characters");
                }
                if (!Regex.IsMatch(client.LastName, @"^[a-zA-Z\s]+$"))
                {
                    return StatusCode(400, "name can't have special characters");
                }
                if (!(Regex.IsMatch(client.Email, @"^([a-zA-Z0-9_.+-])+@(([a-zA-Z0-9-])+.)+([a-zA-Z0-9]{2,4})+$")))
                {
                    return StatusCode(400, "invalid email");
                }
                if (client.Password.Length < 8)
                {
                    return StatusCode(400, "password should be longer than 8");
                }
                if (!Regex.IsMatch(client.Password, @"^(?=.[a-z])(?=.[A-Z])(?=.*\d).+$"))
                {
                    return StatusCode(400, "password should have one upper case, one lower case and a number");
                }
                //buscamos si ya existe el usuario
                Client user = _clientRepository.FindByEmail(client.Email);
                if (user == null)
                {
                    return StatusCode(403, "Email está en uso");
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
                return Created("", newClient);
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
                if (client.Accounts.Count > 2)
                {
                    return StatusCode(403, "Usted ya tiene mas de 3 cuentas");
                }
                var account = _accountsController.Post(client.Id); //creamos lacuenta 
                if (account == null)
                {
                    return StatusCode(500, "Error al crear la cuenta");
                }
                return Created("", account);
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
                    return NotFound();
                }
                Client client = _clientRepository.FindByEmail(email);
                Card newCard = new Card
                {
                    CardHolder = client.FirstName + " " + client.LastName,
                    Type = card.Type,
                    Color = card.Color,
                    Number = new Random().Next(1000, 9999).ToString() + ("-") +
                    new Random().Next(1000, 9999).ToString() + ("-") +
                    new Random().Next(1000, 9999).ToString() + ("-") +
                    new Random().Next(1000, 9999).ToString(),
                    Cvv = new Random().Next(100, 999),
                    FromDate = DateTime.Now,
                    ThruDate = DateTime.Now.AddYears(4),
                    ClientId = client.Id
                };
                if (card.Type != CardType.CREDIT.ToString() && card.Type != CardType.DEBIT.ToString())
                {
                    return BadRequest("El tipo de tarjeta no es valido");
                }
                if (card.Color != CardColor.GOLD.ToString() && card.Color != CardColor.SILVER.ToString() && card.Type != CardColor.SILVER.ToString())
                {
                    return BadRequest("El color de tarjeta no es valido");
                }
                int CardCount = client.Cards.Where(c => c.Type == card.Type).Count();
                if (CardCount > 2)
                {
                    return StatusCode(403, "Ya tiene 3 tarjetas del mismo tipo");
                }
                int sameCard = client.Cards.Where(c => card.Color == card.Color && c.Type == card.Type).Count();
                if (sameCard == 1)
                {
                    return StatusCode(403, "Ya tiene una tarjeta del mismo tipo y color");
                }
                var newCardDto = _cardsController.Post(newCard);
                if (newCard == null)
                {
                    return StatusCode(500, "Error al crear la tarjeta");
                }
                return Created("", newCardDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}