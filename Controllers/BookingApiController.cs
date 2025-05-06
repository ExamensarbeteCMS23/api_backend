using api_backend.Contexts;
using api_backend.Dtos;
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
public class BookingApiController : ControllerBase
{
    private readonly DataContext _context;
    private readonly ILogger<BookingApiController> _logger;
    private readonly UserManager<ApplicationUser> _userManager;

    public BookingApiController(
        DataContext context,
        ILogger<BookingApiController> logger,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _logger = logger;
        _userManager = userManager;
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

    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ActionName("GetAllBookings")]
    public async Task<IActionResult> GetAllBookings()
    {
        try
        {
            _logger.LogInformation("Hämtar alla bokningar");

            var bookings = await _context.Bookings
                .Include(b => b.Customer)
                .Select(b => new
                {
                    Id = b.Id,
                    Date = b.Date.ToString("yyyy-MM-dd"),
                    Time = b.Time.ToString(),
                    Customer = new
                    {
                        Id = b.Customer.Id,
                        Name = $"{b.Customer.CustomerFirstName} {b.Customer.CustomerLastName}"
                    }
                })
                .ToListAsync();

            _logger.LogInformation("Hämtade {Count} bookings", bookings.Count);
            return Ok(bookings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fel vid hämtning av bokningar");
            return StatusCode(500, "Ett fel har uppstått när bokningarna hämtades: " + ex.Message);
        }
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    [ActionName("GetBookingById")]
    public async Task<IActionResult> GetBookingById(int id)
    {
        try
        {
            var booking = await _context.Bookings
                .Include(b => b.Customer)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return NotFound($"Bokning med ID {id} hittades inte");
            }

            var result = new
            {
                Id = booking.Id,
                Date = booking.Date.ToString("yyyy-MM-dd"),
                Time = booking.Time.ToString(),
                Customer = new
                {
                    Id = booking.Customer.Id,
                    Name = $"{booking.Customer.CustomerFirstName} {booking.Customer.CustomerLastName}"
                }
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fel vid hämtning av bokning med ID {BookingId}", id);
            return StatusCode(500, "Ett fel har uppsått vid hämtning av bokningen: " + ex.Message);
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ActionName("CreateBooking")]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto dto)
    {
        if (dto == null)
        {
            return BadRequest("Request body cannot be null");
        }

        _logger.LogInformation("Skapar bokning för kund med ID: {CustomerId}", dto.CustomerId);

        try
        {
            // Verifiera att kund finns
            var customer = await _context.Customers.FindAsync(dto.CustomerId);
            if (customer == null)
            {
                _logger.LogWarning("Kund hittades inte: {CustomerId}", dto.CustomerId);
                return NotFound($"Kund med ID {dto.CustomerId} hittades inte");
            }

            // Parse time
            if (!TimeOnly.TryParse(dto.Time, out TimeOnly parsedTime))
            {
                _logger.LogWarning("Invalid time format: {Time}", dto.Time);
                return BadRequest($"Invalid time format: {dto.Time}. Use HH:mm or HH:mm:ss format");
            }

            // Skapa och spar bokning
            var booking = new BookingEntity
            {
                CustomerId = dto.CustomerId, 
                Date = dto.Date,
                Time = parsedTime
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Booking created with ID: {BookingId}", booking.Id);

            // Om cleaner ID finns, lägg till det till bokningen
            if (dto.CleanerIds != null && dto.CleanerIds.Any())
            {
                foreach (var cleanerId in dto.CleanerIds)
                {
                    // Verifiera att cleaner finns
                    var cleaner = await _context.Employees.FindAsync(cleanerId);
                    if (cleaner != null)
                    {
                        var bookingCleaner = new BookingCleanerEntity
                        {
                            BookingId = booking.Id,
                            CleanerId = cleanerId
                        };
                        _context.BookingCleaner.Add(bookingCleaner);
                        _logger.LogInformation("Added cleaner {CleanerId} to booking {BookingId}", cleanerId, booking.Id);
                    }
                    else
                    {
                        _logger.LogWarning("Cleaner not found: {CleanerId}", cleanerId);
                    }
                }
                await _context.SaveChangesAsync();
            }
                           
            return StatusCode(201, new { bookingId = booking.Id, message = "Booking created successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating booking");
            return StatusCode(500, "An error occurred while creating the booking: " + ex.Message);
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ActionName("DeleteBooking")]
    public async Task<IActionResult> DeleteBooking(int id)
    {
        try
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound($"Bokning med ID {id} hittades inte");
            }

            // Ta bort BookingCleaner först
            var bookingCleaners = await _context.BookingCleaner
                .Where(bc => bc.BookingId == id)
                .ToListAsync();

            foreach (var bc in bookingCleaners)
            {
                _context.BookingCleaner.Remove(bc);
            }

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Booking deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting booking with ID {BookingId}", id);
            return StatusCode(500, "An error occurred while deleting the booking: " + ex.Message);
        }
    }

    // PUT: api/BookingApi/{id}
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ActionName("UpdateBooking")]
    public async Task<IActionResult> UpdateBooking(int id, [FromBody] BookingUpdateDto bookingDto)
    {
        if (bookingDto == null)
            return BadRequest("Booking cannot be null");

        _logger.LogInformation("Attempting to update booking {BookingId}", id);

        try
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Hämta existerande bokning
                var booking = await _context.Bookings.FindAsync(id);
                if (booking == null)
                {
                    _logger.LogWarning("No booking found with id: {BookingId}", id);
                    return NotFound($"No booking found with id: {id}");
                }

                // Uppdatera kund om en ny angetts
                if (bookingDto.CustomerId.HasValue)
                {
                    var customer = await _context.Customers.FindAsync(bookingDto.CustomerId.Value);
                    if (customer == null)
                    {
                        _logger.LogWarning("Customer with ID {CustomerId} not found", bookingDto.CustomerId.Value);
                        return NotFound($"Customer with ID {bookingDto.CustomerId} not found");
                    }

                    booking.CustomerId = customer.Id;
                }

                // Uppdatera om ny datum och tid angetts
                if (bookingDto.Date.HasValue)
                    booking.Date = bookingDto.Date.Value;

                if (!string.IsNullOrEmpty(bookingDto.Time))
                {
                    if (!TimeOnly.TryParse(bookingDto.Time, out TimeOnly parsedTime))
                    {
                        _logger.LogWarning("Invalid time format: {Time}", bookingDto.Time);
                        return BadRequest($"Invalid time format: {bookingDto.Time}. Använd format HH:mm eller HH:mm:ss");
                    }
                    booking.Time = parsedTime;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Booking {BookingId} updated", id);

                // Uppdatera städarkopplingar om nya angetts
                if (bookingDto.CleanerIds != null)
                {
                    // Ta bort befintliga kopplingar
                    var existingBookingCleaners = await _context.BookingCleaner
                        .Where(bc => bc.BookingId == id)
                        .ToListAsync();

                    foreach (var bc in existingBookingCleaners)
                    {
                        _context.BookingCleaner.Remove(bc);
                    }
                    _logger.LogInformation("Removed {Count} existing cleaner connections", existingBookingCleaners.Count);

                    // Lägg till nya kopplingar
                    foreach (var cleanerId in bookingDto.CleanerIds)
                    {
                        var cleaner = await _context.Employees.FindAsync(cleanerId);
                        if (cleaner != null)
                        {
                            _context.BookingCleaner.Add(new BookingCleanerEntity
                            {
                                BookingId = id,
                                CleanerId = cleanerId
                            });
                            _logger.LogInformation("Added cleaner {CleanerId} to booking {BookingId}", cleanerId, id);
                        }
                        else
                        {
                            _logger.LogWarning("Cleaner with ID {CleanerId} not found", cleanerId);
                        }
                    }

                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                _logger.LogInformation("Transaction for updating booking {BookingId} completed", id);

                return Ok(new { message = "Booking updated", bookingId = id });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Transaction error when updating booking {BookingId}", id);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Critical error when updating booking {BookingId}", id);
            return StatusCode(500, $"Error updating booking: {ex.Message}");
        }
    }

    // GET: api/BookingApi/customer/{customerId}
    [HttpGet("customer/{customerId}")]
    [Authorize(Roles = "Admin")]
    [ActionName("GetCustomerBookings")]
    public async Task<IActionResult> GetCustomerBookings(int customerId)
    {
        try
        {
            // Kontrollera att kunden finns
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null)
            {
                _logger.LogWarning("Customer with ID {CustomerId} not found", customerId);
                return NotFound($"Customer with ID {customerId} not found");
            }

            // Hämta kundens bokningar
            var bookings = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.BookingCleaners)
                    .ThenInclude(bc => bc.Cleaner)
                .Where(b => b.CustomerId == customerId)
                .ToListAsync();

            // Formatera svaret
            var customerBookings = bookings.Select(b => new
            {
                Id = b.Id,
                Date = b.Date.ToString("yyyy-MM-dd"),
                Time = b.Time.ToString(),
                Cleaners = b.BookingCleaners.Select(bc => new
                {
                    Id = bc.Cleaner.Id,
                    Name = $"{bc.Cleaner.EmployeeFirstName} {bc.Cleaner.EmployeeLastName}"
                }).ToList()
            }).ToList();

            _logger.LogInformation("Retrieved {Count} bookings for customer {CustomerId}", customerBookings.Count, customerId);
            return Ok(customerBookings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving bookings for customer {CustomerId}", customerId);
            return StatusCode(500, $"Internal error: {ex.Message}");
        }
    }

    // GET: api/BookingApi/cleaner/me
    [HttpGet("cleaner/me")]
    [ActionName("GetMyBookings")]
    public async Task<IActionResult> GetMyBookings()
    {
        try
        {
            var (user, employee) = await GetCurrentUserWithEmployee();
            if (user == null || employee == null)
                return Unauthorized("User or employee not found");

            // Hämta bokningar för inloggad städare
            var bookingIds = await _context.BookingCleaner
                .Where(bc => bc.CleanerId == employee.Id)
                .Select(bc => bc.BookingId)
                .ToListAsync();

            if (!bookingIds.Any())
                return Ok(new List<object>()); 

            var bookings = await _context.Bookings
                .Include(b => b.Customer)
                .Where(b => bookingIds.Contains(b.Id))
                .ToListAsync();

            // Hämta adressinformation
            var customerIds = bookings.Select(b => b.CustomerId).ToList();
            var addressInfo = await _context.Customers
                .Where(c => customerIds.Contains(c.Id))
                .Join(_context.CustomerAddresses,
                    c => c.AddressId,
                    a => a.Id,
                    (c, a) => new { CustomerId = c.Id, Address = a })
                .ToDictionaryAsync(x => x.CustomerId, x => x.Address);

            // Formatera svaret
            var myBookings = bookings.Select(b => new
            {
                Id = b.Id,
                Date = b.Date.ToString("yyyy-MM-dd"),
                Time = b.Time.ToString(),
                Customer = new
                {
                    Name = $"{b.Customer.CustomerFirstName} {b.Customer.CustomerLastName}",
                    Address = addressInfo.TryGetValue(b.CustomerId, out var addr)
                        ? $"{addr.CustomerStreetName}, {addr.CustomerPostalCode} {addr.CustomerCity}"
                        : "Address missing"
                }
            }).ToList();

            _logger.LogInformation("Retrieved {Count} bookings for cleaner {CleanerId}", myBookings.Count, employee.Id);
            return Ok(myBookings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cleaner's bookings");
            return StatusCode(500, $"Internal error: {ex.Message}");
        }
    }
}