using api_backend.Contexts;
using api_backend.Dtos;
using api_backend.Interfaces;
using api_backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace api_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BookingController : ControllerBase
{
    private readonly ILogger<BookingController> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IBookingService _bookingService;
    private readonly DataContext _context; // Behövs för GetCurrentUserWithEmployee

    public BookingController(
        ILogger<BookingController> logger,
        UserManager<ApplicationUser> userManager,
        IBookingService bookingService,
        DataContext context)
    {
        _logger = logger;
        _userManager = userManager;
        _bookingService = bookingService;
        _context = context;
    }

    // Hjälpmetod för att hämta nuvarande användare med Employee-data
    private async Task<(ApplicationUser? User, EmployeeEntity? Employee)> GetCurrentUserWithEmployee()
    {
        // Hämtar alla claims av typen NameIdentifier
        var userIdClaims = User.FindAll(ClaimTypes.NameIdentifier).ToList();
        _logger.LogInformation("Found {Count} nameidentifier claims", userIdClaims.Count);

        //Försök omvandla claim-värdet till ett GUID och använd det som användar-ID
        string? userId = null;
        foreach (var claim in userIdClaims)
        {
            if (Guid.TryParse(claim.Value, out _))
            {
                userId = claim.Value;
                break;
            }
        }

        _logger.LogInformation("Selected UserId: {UserId}", userId);

        if (string.IsNullOrEmpty(userId))
            return (null, null);

        var user = await _userManager.FindByIdAsync(userId);
        _logger.LogInformation("Found user: {HasUser}, EmployeeId: {EmployeeId}",
            user != null, user?.EmployeeId);

        if (user == null)
            return (null, null);

        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == user.EmployeeId);
        _logger.LogInformation("Found employee: {HasEmployee}", employee != null);

        return (user, employee);
    }

    [HttpGet("GetAllBookings")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllBookings()
    {
        try
        {
            _logger.LogInformation("Hämtar alla bokningar");
            var bookings = await _bookingService.GetAllBookingsAsync();
            _logger.LogInformation("Hämtade bokningar via service");
            return Ok(bookings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fel vid hämtning av bokningar");
            return StatusCode(500, "Ett fel har uppstått när bokningarna hämtades: " + ex.Message);
        }
    }

    [HttpGet("GetBookingById{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetBookingById(int id)
    {
        try
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);

            if (booking == null)
            {
                return NotFound($"Bokning med ID {id} hittades inte");
            }

            return Ok(booking);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fel vid hämtning av bokning med ID {BookingId}", id);
            return StatusCode(500, "Ett fel har uppsått vid hämtning av bokningen: " + ex.Message);
        }
    }

    [HttpPost("RegisterBooking")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto dto)
    {
        if (dto == null)
        {
            return BadRequest("Request body cannot be null");
        }

        _logger.LogInformation("Skapar bokning för kund med ID: {CustomerId}", dto.CustomerId);

        try
        {
            var bookingId = await _bookingService.CreateBookingAsync(dto);
            _logger.LogInformation("Booking created with ID: {BookingId}", bookingId);

            return StatusCode(201, new { bookingId = bookingId, message = "Booking created successfully" });
        }
        catch (api_backend.Services.NotFoundException ex)
        {
            _logger.LogWarning("Resource not found: {Message}", ex.Message);
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid input: {Message}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating booking");
            return StatusCode(500, "An error occurred while creating the booking: " + ex.Message);
        }
    }

    [HttpDelete("DeleteBooking{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteBooking(int id)
    {
        try
        {
            var result = await _bookingService.DeleteBookingAsync(id);

            if (!result)
            {
                return NotFound($"Bokning med ID {id} hittades inte");
            }

            return Ok(new { message = "Booking deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting booking with ID {BookingId}", id);
            return StatusCode(500, "An error occurred while deleting the booking: " + ex.Message);
        }
    }

    // PUT: api/BookingApi/{id}
    [HttpPut("UpdateBooking{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateBooking(int id, [FromBody] BookingUpdateDto bookingDto)
    {
        if (bookingDto == null)
            return BadRequest("Booking cannot be null");

        _logger.LogInformation("Attempting to update booking {BookingId}", id);

        try
        {
            var result = await _bookingService.UpdateBookingAsync(id, bookingDto);

            if (!result)
            {
                return NotFound($"No booking found with id: {id}");
            }

            return Ok(new { message = "Booking updated", bookingId = id });
        }
        catch (api_backend.Services.NotFoundException ex)
        {
            _logger.LogWarning("Resource not found: {Message}", ex.Message);
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid input: {Message}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Critical error when updating booking {BookingId}", id);
            return StatusCode(500, $"Error updating booking: {ex.Message}");
        }
    }

    // GET: api/BookingApi/customer/{customerId}
    [HttpGet("GetCustomerBookings/{customerId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetCustomerBookings(int customerId)
    {
        try
        {
            var customerBookings = await _bookingService.GetCustomerBookingsAsync(customerId);
            return Ok(customerBookings);
        }
        catch (api_backend.Services.NotFoundException ex)
        {
            _logger.LogWarning("Customer not found: {Message}", ex.Message);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving bookings for customer {CustomerId}", customerId);
            return StatusCode(500, $"Internal error: {ex.Message}");
        }
    }
    // GET: api/BookingApi/cleaner/me
    [HttpGet("GetMyBookings/Cleaner/me")]
    public async Task<IActionResult> GetMyBookings()
    {
        try
        {
            var (user, employee) = await GetCurrentUserWithEmployee();
            if (user == null || employee == null)
                return Unauthorized("User or employee not found");

            // Går mot servicen
            var myBookings = await _bookingService.GetMyBookingsAsync(employee.Id);

            _logger.LogInformation("Retrieved bookings for cleaner {CleanerId}", employee.Id);
            return Ok(myBookings);
        }
        catch (api_backend.Services.NotFoundException ex)
        {
            _logger.LogWarning("Resource not found: {Message}", ex.Message);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cleaner's bookings");
            return StatusCode(500, $"Internal error: {ex.Message}");
        }
    }
}