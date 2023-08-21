using HomeBankingMindHub.Models;
using HomeBankingMindHub.Models.DTO;
using HomeBankingMindHub.Models.Enum;
using HomeBankingMindHub.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HomeBankingMindHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoansController : ControllerBase
    {

        private IClientRepository _clientRepository;
        private IAccountRepository _accountRepository;
        private ILoanRepository _loanRepository;
        private IClientLoanRepository _clientLoanRepository;
        private ITransactionRepository _transactionRepository;

        public LoansController(IClientRepository clientRepository, IAccountRepository accountRepository, ILoanRepository loanRepository, IClientLoanRepository clientLoanRepository, ITransactionRepository transactionRepository)
        {
            _clientRepository = clientRepository;
            _accountRepository = accountRepository;
            _loanRepository = loanRepository;
            _clientLoanRepository = clientLoanRepository;
            _transactionRepository = transactionRepository;
        }

        [HttpPost]
        public IActionResult Post([FromBody] LoanApplicationDTO loanApplicationDTO)
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
                var loan = _loanRepository.FindById(loanApplicationDTO.LoanId);
                if (loan == null)
                {
                    return StatusCode(403, "El prestamo no existe");
                }
                if (string.IsNullOrEmpty(loanApplicationDTO.Payments) || loanApplicationDTO.Amount <= 0 || string.IsNullOrEmpty(loanApplicationDTO.ToAccountNumber) || loanApplicationDTO.Payments == "0")
                {
                    return StatusCode(403, "Por favor complete todos los campos");
                }
                if (loanApplicationDTO.Amount > loan.MaxAmount)
                {
                    return StatusCode(403, "El monto solicitado supera el monto maximo permitido del prestamo solicitado");
                }
                var paymentList = loan.Payments.Split(',');
                if (!paymentList.Contains(loanApplicationDTO.Payments))
                {
                    return StatusCode(403, "La cantidad de cuotas no esta disponible para el prestamo solicitado");
                }
                var account = _accountRepository.FindByNumber(loanApplicationDTO.ToAccountNumber);
                if (account == null)
                {
                    return StatusCode(403, "La cuenta destino no existe");
                }
                if (account.ClientId != client.Id)
                {
                    return StatusCode(403, "La cuenta destino no pertenece al cliente");
                }
                var acc = client.Accounts.Where(acc => acc.Number == account.Number).FirstOrDefault();
                if (acc == null)
                {
                    return StatusCode(403, "Las cuentas no coinciden");
                }
                account.Balance += loanApplicationDTO.Amount;
                _accountRepository.Save(account);
                Transaction transaction = new Transaction
                {
                    Type = TransactionType.CREDIT.ToString(),
                    Amount = loanApplicationDTO.Amount,
                    Description = loan.Name.ToString() + " loan approved",
                    Date = DateTime.Now,
                    AccountId = account.Id
                };
                _transactionRepository.Save(transaction);
                ClientLoan clientLoan = new ClientLoan
                {
                    Amount = loanApplicationDTO.Amount + (loanApplicationDTO.Amount * 0.2),
                    Payments = loanApplicationDTO.Payments,
                    ClientId = client.Id,
                    LoanId = loan.Id
                };
                _clientLoanRepository.Save(clientLoan);
                return Ok(loanApplicationDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                var loans = _loanRepository.GetAllLoans();
                var loansDTO = new List<LoanDTO>();
                foreach (Loan loan in loans)
                {
                    var newLoanDTO = new LoanDTO
                    {
                        Id = loan.Id,
                        Name = loan.Name,
                        MaxAmount = loan.MaxAmount,
                        Payments = loan.Payments,
                    };
                    loansDTO.Add(newLoanDTO);
                }
                return Ok(loansDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

    }
}
