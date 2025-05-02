using api_backend.Contexts;
using api_backend.Interfaces;
using api_backend.Models;
using api_backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace api_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController(IEmployeeService employeeService) : ControllerBase
    {
        private readonly IEmployeeService _employeeService = employeeService;


        // Registrera en användare, får bara göras av en Admin
        //[HttpPost("RegisterEmployee")]
        //[Authorize(Roles = "Admin")]
        //public async Task<IActionResult> RegisterEmployee([FromBody] RegisterCleanerDto dto)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState);

        //    // Kontrollera om email redan finns
        //    var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        //    if (existingUser != null)
        //        return BadRequest("Användare med den emailen finns redan.");

        //    using var transaction = await _context.Database.BeginTransactionAsync();
        //    try
        //    {
        //        // 1. Skapa CleanerEntity
        //        var employee = new EmployeeEntity
        //        {
        //            EmployeeFirstName = dto.FirstName,
        //            EmployeeLastName = dto.LastName,
        //            EmployeeEmail = dto.Email,
        //            EmployeePhone = dto.Phone,
        //            RoleId = dto.RoleId
        //        };

        //        _context.Employees.Add(employee);

        //        // Spara Cleaner
        //        await _context.SaveChangesAsync();

        //        // Skapa ApplicationUser kopplat till CleanerEntity
        //        var user = new ApplicationUser
        //        {
        //            UserName = dto.Email,
        //            Email = dto.Email,
        //            EmployeeId = employee.Id
        //        };

        //        var result = await _userManager.CreateAsync(user, dto.Password);

        //        if (!result.Succeeded)
        //        {
        //            await transaction.RollbackAsync();
        //            var errorMessages = result.Errors.Select(e => e.Description);
        //            return BadRequest(new { message = "Kunde inte skapa användare", errors = errorMessages });
        //        }

        //        var roleName = await _context.Roles
        //            .Where(r => r.Id == dto.RoleId)
        //        .Select(r => r.Role)
        //        .FirstOrDefaultAsync();

        //        if (!string.IsNullOrEmpty(roleName))
        //        {
        //            await _userManager.AddToRoleAsync(user, roleName);
        //        }

        //        await transaction.CommitAsync();
        //        return Ok(new { Message = "Anställd registrerad och inloggningsbar!", CleanerId = employee.Id });
        //    }
        //    catch (Exception ex)
        //    {
        //        await transaction.RollbackAsync();
        //        var errorMessage = ex.Message;

        //        if (ex.InnerException != null)
        //        {
        //            errorMessage += " InnerException: " + ex.InnerException.Message;
        //        }

        //        return StatusCode(500, "Internt fel vid registrering: " + errorMessage);
        //    }

        //}

        // Hämtar alla anställda, enbart tillåtet för en Admin
        [HttpGet("GetAllEmployee")]
        public async Task<IActionResult> GetAllEmployee()
        {
            try
            {
                // Hämta från databasen
                var employees = await _employeeService.GetAllAsync();
                return Ok(employees);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Felet är: {ex.Message}");
                return StatusCode(500, new { message = "Internt fel", error = ex.Message });
            }
        }

        //[HttpGet("GetEmployeeById")]
        //public async Task<IActionResult> GetEmployeeById(int id)
        //{
        //    try
        //    {
        //        // Kontrollera att användaren finns i databasen baserat på id
        //        var employee = await _context.Employees.FirstOrDefaultAsync(x => x.Id == id);

        //        if (employee == null)
        //        {
        //            return NotFound($"Anställd med anställningsnummer {id} kunde inte hittas");
        //        }
        //        return Ok(employee);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Fel vid hämtning av anställd: {ex.Message}");
        //        return StatusCode(500, new { message = "Ett fel inträffade", error = ex.Message });
        //    }
        //}

        //[HttpPut("UpdateEmployee/{id}")]
        //public async Task<IActionResult> UpdateEmployee(int id, [FromBody] UpdateEmployeeDto dto)
        //{
        //    // Hämta ut den anställda från databasen
        //    var employee = await _context.Employees.FirstOrDefaultAsync(x => x.Id == id);
        //    if (employee == null)
        //    {
        //        return NotFound(new { message = "Anställd kunde inte hittas" });
        //    }

        //    // Hämta Användaren om den finns i Identity
        //    var user = await _userManager.Users.FirstOrDefaultAsync(u => u.EmployeeId == employee.Id);
        //    if (user == null)
        //    {
        //        return NotFound(new { message = "Användarkontot kunde inte hittas" });
        //    }


        //    if (!string.IsNullOrEmpty(dto.Email) && dto.Email != user.Email)
        //    {
        //        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        //        if (existingUser != null)
        //        {
        //            return BadRequest(new { message = "Den nya eposten används redan" });
        //        }

        //        var emailResult = await _userManager.SetEmailAsync(user, dto.Email);
        //        var usernameResult = await _userManager.SetUserNameAsync(user, dto.Email);

        //        if (!usernameResult.Succeeded || !emailResult.Succeeded)
        //        {
        //            return BadRequest(new
        //            {
        //                message = "Det gick inte att uppdatera e-postadressen.",
        //                errors = emailResult.Errors.Concat(usernameResult.Errors).Select(e => e.Description)
        //            });
        //        }
        //        employee.EmployeeEmail = dto.Email;
        //    }
        //    // Uppdatera andra fält om de skickas
        //    if (!string.IsNullOrEmpty(dto.FirstName))
        //        employee.EmployeeFirstName = dto.FirstName;
        //    if (!string.IsNullOrEmpty(dto.LastName))
        //        employee.EmployeeLastName = dto.LastName;
        //    if (!string.IsNullOrEmpty(dto.Phone))
        //        employee.EmployeePhone = dto.Phone;
        //    if (dto.RoleId.HasValue)
        //        employee.RoleId = dto.RoleId.Value;

        //    await _context.SaveChangesAsync();
        //    return Ok(new { message = "Anställd uppdaterad." });

        //}
    }

}
