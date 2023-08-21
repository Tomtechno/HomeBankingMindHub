using HomeBankingMindHub.Models;
using HomeBankingMindHub.Models.DTO;
using HomeBankingMindHub.Models.Enum;
using HomeBankingMindHub.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System;
using System.Security.Principal;

namespace HomeBankingMindHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {

        private IClientRepository _clientRepository;
        private IAccountRepository _accountRepository;
        private ITransactionRepository _transactionRepository;


        public TransactionsController(IClientRepository clientRepository, IAccountRepository accountRepository, ITransactionRepository transactionRepository)
        {
            _clientRepository = clientRepository;
            _accountRepository = accountRepository;
            _transactionRepository = transactionRepository;
        }

        [HttpPost]
        public IActionResult Post([FromBody] TransferDTO transferDTO)
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
                if (transferDTO.FromAccountNumber == string.Empty || transferDTO.ToAccountNumber == string.Empty)
                {
                    return StatusCode(403, "Cuenta de origen o cuenta de destino no proporcionada");
                }
                if (transferDTO.FromAccountNumber == transferDTO.ToAccountNumber)
                {
                    return StatusCode(403, "No se permite la transferencia a la misma cuenta");
                }
                if (transferDTO.Amount == 0 || transferDTO.Description == string.Empty)
                {
                    return StatusCode(403, "Monto o descripcion no proporcionados");
                }
                if (transferDTO.Amount < 0)
                {
                    return StatusCode(403, "El monto debe ser mayor a cero");
                }
                Account fromAccount = _accountRepository.FindByNumber(transferDTO.FromAccountNumber);
                if (fromAccount == null)
                {
                    return StatusCode(403, "Cuenta de origen no existe");
                }
                if (fromAccount.Balance < transferDTO.Amount)
                {
                    return StatusCode(403, "Fondos insuficientes");
                }
                Account toAccount = _accountRepository.FindByNumber(transferDTO.ToAccountNumber);
                if (toAccount == null)
                {
                    return StatusCode(403, "Cuenta de destino no existe");
                }
                //demas logica para guardado
                //comenzamos con la inserción de las 2 transacciones realizadas
                //desde toAccount se debe generar un debito por lo tanto lo multiplicamos por -1
                _transactionRepository.Save(new Transaction //Cuenta Origen
                {
                    Type = TransactionType.DEBIT.ToString(),
                    Amount = transferDTO.Amount * -1,
                    Description = transferDTO.Description + " " + toAccount.Number,
                    AccountId = fromAccount.Id,
                    Date = DateTime.Now,
                });
                //ahora una credito para la cuenta fromAccount
                _transactionRepository.Save(new Transaction //Cuenta Destino
                {
                    Type = TransactionType.CREDIT.ToString(),
                    Amount = transferDTO.Amount,
                    Description = transferDTO.Description + " " + fromAccount.Number,
                    AccountId = toAccount.Id,
                    Date = DateTime.Now,
                });
                fromAccount.Balance = fromAccount.Balance - transferDTO.Amount;
                _accountRepository.Save(fromAccount);
                toAccount.Balance = toAccount.Balance + transferDTO.Amount;
                _accountRepository.Save(toAccount);
                return Created("Creado con exito", fromAccount);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

    }
}
