using HomeBankingMindHub.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using HomeBankingMindHub.Models;
using HomeBankingMindHub.dtos;

namespace HomeBankingMindHub.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private IClientRepository _clientRepository;
        public AuthController(IClientRepository clientRepository)
        {
            _clientRepository = clientRepository;
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Client client)
        {
            try
            {
                Client user = _clientRepository.FindByEmail(client.Email);
                if (user == null || !String.Equals(user.Password, client.Password))
                    return Unauthorized();
                var claims = new List<Claim>
                {
                    new Claim("Client", user.Email), // Un claim es un atributo/propiedad asociado a la identidad del usuario 
                };
                var claimsIdentity = new ClaimsIdentity( // Es propio de cada usuario y tiene una lista con sus claims. Representa a un usuario autenticado
                    claims,
                    CookieAuthenticationDefaults.AuthenticationScheme 
                    );
                await HttpContext.SignInAsync( // Le pasamos todos los datos que configuramos con la identidad de la persona para que cree la cookie la mande al navegador y la persona cuando se loguee
                    // y cada vez que la persona hace una peticion al back esa cookie tambien viaja
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity));
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}