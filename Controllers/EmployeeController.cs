using api_backend.Contexts;
using api_backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController(UserManager<ApplicationUser> userManager, IConfiguration config, DataContext context) : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly IConfiguration _config = config;
        private readonly DataContext _context = context;

        [HttpPost("RegisterEmployee")]
        [Authorize(Roles = "Admin")]
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
