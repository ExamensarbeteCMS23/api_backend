using api_backend.Interfaces;
using api_backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace api_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        private readonly IAuthService _authService = authService;

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Ogiltig begäran");
            }
            try
            {
                var (token, expiration) = await _authService.AuthenticateAsync(model.Email, model.Password);
                return Ok(new
                {
                    token,
                    expiration
                });
            }
            catch (UnauthorizedAccessException ex) { return Unauthorized(ex.Message); }
            catch (Exception)
            {
                return StatusCode(500, "Ett internt fel inträffade vid inloggning");
            }
        }
    }
}
