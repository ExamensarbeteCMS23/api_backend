using api_backend.Dtos;
using api_backend.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController(IEmployeeService employeeService) : ControllerBase
    {
        private readonly IEmployeeService _employeeService = employeeService;


        // Registrera en användare, får bara göras av en Admin
        [HttpPost("RegisterEmployee")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> RegisterEmployee([FromBody] RegisterCleanerDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _employeeService.RegisterEmployeeAsync(dto);
            if(!result.Success)
                return BadRequest(new { result.Message, result.Errors});

            return Ok(new { result.Message, CleanderId = result.Data });

        }

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

        [HttpDelete("RemoveEmployee")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var result = await _employeeService.DeleteAsync(id);
            if (result != null)
            {
                return Ok(result);
            }
            return BadRequest();
        }
    }

}
