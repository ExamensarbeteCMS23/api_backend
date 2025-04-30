using api_backend.Contexts;
using api_backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace api_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;
        private readonly DataContext _context;

        public AuthController(UserManager<ApplicationUser> userManager, IConfiguration config, DataContext context)
        {
            _userManager = userManager;
            _config = config;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                return Unauthorized("Fel användare eller lösenord");

            var roles = await _userManager.GetRolesAsync(user);
            


            var authClaims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
            };

            foreach (var role in roles) {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["authToken:Key"]));

            Console.WriteLine("LOGIN: Issuer = " + "testissuer");
            Console.WriteLine("LOGIN: Audience = " + "testaudience");
            Console.WriteLine("LOGIN: Key = " + _config["authToken:Key"]);

            var token = new JwtSecurityToken(
                issuer: "testissuer",
                audience: "testaudience",
                expires: DateTime.UtcNow.AddHours(4),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            string tokenString;
            try
            {
                tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fel vid WriteToken:");
                Console.WriteLine(ex.Message);
                if (ex.InnerException != null)
                    Console.WriteLine(ex.InnerException.Message);
                throw; // eller returnera ett felmeddelande
            }

            return Ok(new
            {
                token = tokenString,
                expiration = token.ValidTo,
            });
        }
    }
}
