using api_backend.Contexts;
using api_backend.Models;
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
            _context = context;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                return Unauthorized("Fel användare eller lösenord");

            var userRoles = _userManager.GetRolesAsync(user);

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["authToken:Key"]));

            var authClaims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            authClaims.AddRange(userRoles.Select(RoleEntity => new Claim(ClaimTypes.Role, role)));

            var token = new JwtSecurityToken(
                issuer: "testissuer",
                audience: "testaudience",
                expires: DateTime.UtcNow.AddHours(1),
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
                expiration = token.ValidTo
            });
        }

        [HttpPost("RegisterEmployee")]
        public async Task<IActionResult> RegisterEmployee([FromBody] RegisterCleanerDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Kontrollera om email redan finns
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return BadRequest("Användare med den emailen finns redan.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Skapa CleanerEntity
                var employee = new EmployeeEntity
                {
                    EmployeeFirstName = dto.FirstName,
                    EmployeeLastName = dto.LastName,
                    EmployeeEmail = dto.Email,
                    EmployeePhone = dto.Phone,
                    RoleId = dto.RoleId
                };

                _context.Employees.Add(employee);

                // Spara Cleaner
                await _context.SaveChangesAsync();

                // 2. Skapa ApplicationUser kopplat till CleanerEntity
                var user = new ApplicationUser
                {
                    UserName = dto.Email,
                    Email = dto.Email,
                    EmployeeId = employee.Id
                };

                var result = await _userManager.CreateAsync(user, dto.Password);

                if (!result.Succeeded)
                {
                    await transaction.RollbackAsync();
                    var errorMessages = result.Errors.Select(e => e.Description);
                    return BadRequest(new { message = "Kunde inte skapa användare", errors = errorMessages });
                }

                var roleName = await _context.Roles
                    .Where(r => r.Id == dto.RoleId)
                    .Select(r => r.Role)
                    .FirstOrDefaultAsync();

                if (!string.IsNullOrEmpty(roleName))
                {
                    await _userManager.AddToRoleAsync(user, roleName);
                }

                await transaction.CommitAsync();
                return Ok(new { Message = "Anställd registrerad och inloggningsbar!", CleanerId = employee.Id });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                var errorMessage = ex.Message;

                if (ex.InnerException != null)
                {
                    errorMessage += " InnerException: " + ex.InnerException.Message;
                }

                return StatusCode(500, "Internt fel vid registrering: " + errorMessage);
            }

        }


    }
}
