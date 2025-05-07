using api_backend.Dtos;
using api_backend.Interface;
using api_backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController(ICustomerService customerService) : ControllerBase
    {
        private readonly ICustomerService _customerService = customerService;

        [HttpPost("RegisterCustomer")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RegisterCustomer([FromBody] CreateCustomerRequestDto dto)
        {
            var result = await _customerService.RegisterCustomerAsync(dto);
            if (result == null)
                return StatusCode(500, "Kunde inte skapa kunden");

            return Ok(result);
        }

        [HttpGet("GetAllCustomers")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllCustomers()
        {
            var customers = await _customerService.GetAllCustomersAsync();
            if (customers == null || !customers.Any())
            {
                return NotFound(new { message = "Inga kunder kunde hittas" });
            }
            return Ok(customers);
        }

        [HttpGet("GetCustomerById{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetCustomerAsync(int id)
        {
            var customer = await _customerService.GetCustomerAsync(id);
            if (customer == null)
            {
                return NotFound(new { message = "Kunden kunde inte hittas" });
            }
            return Ok(customer);
        }

        [HttpDelete("DeleteCustomer{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var result = await _customerService.RemoveCustomerAsync(id);
            if (!result.Success)
                return NotFound(result.Message);
            return Ok(result.Message);
        }

        [HttpPut("UpdateCustomer{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] UpdateCustomerDto dto)
        {
            var result = await _customerService.UpdateCustomerAsync(id, dto);
            if (!result.Success)
            {
                return NotFound(result.Message);
            }
            return Ok(result.Data);
        }
    }
}
