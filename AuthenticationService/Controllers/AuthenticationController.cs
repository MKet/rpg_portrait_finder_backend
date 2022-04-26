using AuthenticationLibrary.Models;
using AuthenticationLibrary.Models.Output;
using AuthenticationService.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _service;

        public AuthenticationController(IAuthenticationService service) => _service = service;

        [HttpPost]
        public async Task<ActionResult<AuthToken>> Authenticate([FromBody] AuthenticationData authenticationData)
        {
            var token = await _service.LoginAsync(authenticationData.Username, authenticationData.Password);

            if (token is not null) 
            {
                return Ok(token);
            }
            else
            {
                return Unauthorized();
            }

        }
    }
}
