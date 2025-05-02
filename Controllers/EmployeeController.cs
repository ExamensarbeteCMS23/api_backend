using api_backend.Interfaces;
using api_backend.Models;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> GetAllEmployees()
        {
            var employees = await _employeeService.GetAllAsync();

            if (employees == null || !employees.Any())
            {
                return NotFound(new { message = "Inga anställda kunde hittas" });
            }
            return Ok(employees);
        }

        [HttpGet("GetEmployeeById")]
        public async Task<IActionResult> GetEmployeeById(int id)
        {
            var employee = await _employeeService.GetByIdAsync(id);
            return employee is null ? NotFound(new { message = $"Anställd med Id{id}" }) : Ok(employee);
        }

        [HttpPut("UpdateEmployee/{id}")]
        public async Task<IActionResult> UpdateEmployee(int id, [FromBody] UpdateEmployeeDto dto)
        {
            var result = await _employeeService.UpdateEmployeeAsync(id, dto);
            if (!result.Success)
            {
                return BadRequest(new
                {
                    message = result.Message,
                    errors = result.Errors
                });
            }

            return Ok(new { message = result.Message, updated = result.Data });

        }
    }

}
