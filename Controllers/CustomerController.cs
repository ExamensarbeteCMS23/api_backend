using api_backend.Dtos;
using api_backend.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.AccessControl;

namespace api_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController(ICustomerService customerService) : ControllerBase
    {
        private readonly ICustomerService _customerService = customerService;

        [HttpGet("GetAllCustomers")]
        public async Task<IActionResult> GetAllCustomers()
        {
            var customers = await _customerService.GetAllCustomersAsync();
            if (customers == null || !customers.Any()) { 
                return NotFound(new { message = "Inga kunder kunde hittas" });
            }
            return Ok(customers);
        }

        [HttpPost("RegisterCustomer")]
        public async Task<IActionResult> RegisterCustomer ([FromBody] CreateCustomerRequestDto dto)
        {
            var result = await _customerService.RegisterCustomerAsync(dto);
            if (result == null)
                return StatusCode(500, "Kunde inte skapa kunden");

            return Ok(result);
        }

        
    }
}
