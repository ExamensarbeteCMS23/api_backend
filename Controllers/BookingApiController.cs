using api_backend.Contexts;
using api_backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace api_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Kräver autentisering för alla endpoints
    public class BookingApiController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BookingApiController(DataContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Hjälpmetod för att hämta nuvarande användare med Employee-data
        private async Task<(ApplicationUser User, EmployeeEntity Employee)> GetCurrentUserWithEmployee()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return (null, null);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return (null, null);

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == user.EmployeeId);

            return (user, employee);
        }

        // Hjälpmetod för att kontrollera om användaren är Admin
        private async Task<bool> IsAdmin()
        {
            var roles = await _userManager.GetRolesAsync(
                await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)));
            return roles.Contains("Admin");
        }

        [HttpGet("seed")]
        [Authorize(Roles = "Admin")]
        [ActionName("Seed testdata")]
        public async Task<IActionResult> SeedTestData()
        {
            if (await _context.Bookings.AnyAsync())
                return Ok("Testdata finns redan.");

            try
            {
                // 1. Skapa adress
                var address = new CustomerAddressEntity
                {
                    CustomerStreetName = "Testgatan 1",
                    CustomerCity = "Stockholm",
                    CustomerPostalCode = "12345"
                };
                _context.CustomerAddresses.Add(address);
                await _context.SaveChangesAsync();

                // 2. Skapa kund
                var customer = new CustomerEntity
                {
                    CustomerFirstName = "Tina",
                    CustomerLastName = "Lutti",
                    AddressId = address.Id
                };
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                // 3. Skapa roll (om det inte redan finns)
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.Role == "Cleaner");
                if (role == null)
                {
                    role = new RoleEntity { Role = "Cleaner" };
                    _context.Roles.Add(role);
                    await _context.SaveChangesAsync();
                }

                // 4. Skapa employee/cleaner
                var employee = new EmployeeEntity
                {
                    EmployeeFirstName = "Anna",
                    EmployeeLastName = "Städsson",
                    EmployeeEmail = "anna@clean.com",
                    EmployeePhone = "0701234567",
                    RoleId = role.Id
                };
                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();

                // 5. Skapa bokning
                var booking = new BookingEntity
                {
                    Customer = customer,
                    Date = DateTime.Today.AddDays(1),
                    Time = new TimeOnly(9, 0)
                };
                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                // 6. Skapa koppling mellan bokning och städare
                var bookingCleaner = new BookingCleanerEntity
                {
                    BookingId = booking.Id,
                    CleanerId = employee.Id
                };
                _context.BookingCleaner.Add(bookingCleaner);
                await _context.SaveChangesAsync();

                return Ok($"Testdata skapad! Städare ID: {employee.Id}, Bokning ID: {booking.Id}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Fel vid skapande av testdata: {ex.Message} - {ex.InnerException?.Message}");
            }
        }

        [HttpGet]
        [ActionName("Hämta alla bokningar")]
        public async Task<IActionResult> GetAllBookings()
        {
            try
            {
                var (user, employee) = await GetCurrentUserWithEmployee();
                if (user == null || employee == null)
                    return Unauthorized("Användare hittades inte");

                var isAdmin = await IsAdmin();

                if (isAdmin)
                {
                    // Admin får se alla bokningar med full information
                    var bookings = await _context.Bookings
                        .Include(b => b.Customer)
                        .Include(b => b.BookingCleaners)
                            .ThenInclude(bc => bc.Cleaner)
                        .ToListAsync();

                    // Hämta också adressinformation för varje kund
                    var customerIds = bookings.Select(b => b.Customer.Id).ToList();
                    var addressInfo = await _context.Customers
                        .Where(c => customerIds.Contains(c.Id))
                        .Join(_context.CustomerAddresses,
                            c => c.AddressId,
                            a => a.Id,
                            (c, a) => new { CustomerId = c.Id, Address = a })
                        .ToDictionaryAsync(x => x.CustomerId, x => x.Address);

                    // Bygg en mer detaljerad respons
                    var bookingsDetails = bookings.Select(b => new
                    {
                        Id = b.Id,
                        Date = b.Date.ToString("yyyy-MM-dd"),
                        Time = b.Time.ToString(),
                        Customer = new
                        {
                            Id = b.Customer.Id,
                            Name = $"{b.Customer.CustomerFirstName} {b.Customer.CustomerLastName}",
                            Address = addressInfo.TryGetValue(b.Customer.Id, out var addr)
                                ? $"{addr.CustomerStreetName}, {addr.CustomerPostalCode} {addr.CustomerCity}"
                                : "Adress saknas"
                        },
                        Cleaners = b.BookingCleaners.Select(bc => new
                        {
                            Id = bc.Cleaner.Id,
                            Name = $"{bc.Cleaner.EmployeeFirstName} {bc.Cleaner.EmployeeLastName}",
                            Email = bc.Cleaner.EmployeeEmail,
                            Phone = bc.Cleaner.EmployeePhone
                        }).ToList()
                    }).ToList();

                    return Ok(bookingsDetails);
                }
                else
                {
                    // Städare får bara se sina egna bokningar
                    var cleanerId = employee.Id;

                    var bookingIds = await _context.BookingCleaner
                        .Where(bc => bc.CleanerId == cleanerId)
                        .Select(bc => bc.BookingId)
                        .ToListAsync();

                    if (!bookingIds.Any())
                        return Ok(new List<object>()); // Returnera tom lista om inga bokningar finns

                    var bookings = await _context.Bookings
                        .Include(b => b.Customer)
                        .Where(b => bookingIds.Contains(b.Id))
                        .ToListAsync();

                    // Hämta adressinformation
                    var customerIds = bookings.Select(b => b.Customer.Id).ToList();
                    var addressInfo = await _context.Customers
                        .Where(c => customerIds.Contains(c.Id))
                        .Join(_context.CustomerAddresses,
                            c => c.AddressId,
                            a => a.Id,
                            (c, a) => new { CustomerId = c.Id, Address = a })
                        .ToDictionaryAsync(x => x.CustomerId, x => x.Address);

                    // Returnera begränsad information för städaren
                    var cleanerBookings = bookings.Select(b => new
                    {
                        Id = b.Id,
                        Date = b.Date.ToString("yyyy-MM-dd"),
                        Time = b.Time.ToString(),
                        Customer = new
                        {
                            Name = $"{b.Customer.CustomerFirstName} {b.Customer.CustomerLastName}",
                            Address = addressInfo.TryGetValue(b.Customer.Id, out var addr)
                                ? $"{addr.CustomerStreetName}, {addr.CustomerPostalCode} {addr.CustomerCity}"
                                : "Adress saknas"
                        }
                    }).ToList();

                    return Ok(cleanerBookings);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internt fel: {ex.Message} - {ex.InnerException?.Message}");
            }
        }

        [HttpGet("{id}")]
        [ActionName("Hämta en specifik bokning")]
        public async Task<IActionResult> GetBookingById(int id)
        {
            try
            {
                var (user, employee) = await GetCurrentUserWithEmployee();
                if (user == null || employee == null)
                    return Unauthorized("Användare hittades inte");

                var booking = await _context.Bookings
                    .Include(b => b.Customer)
                    .Include(b => b.BookingCleaners)
                        .ThenInclude(bc => bc.Cleaner)
                    .FirstOrDefaultAsync(b => b.Id == id);

                if (booking == null)
                    return NotFound($"Ingen bokning hittades med id: {id}");

                // Hämta adressinformation
                var address = await _context.CustomerAddresses
                    .FirstOrDefaultAsync(a => a.Id == booking.Customer.AddressId);

                var isAdmin = await IsAdmin();
                if (isAdmin)
                {
                    // Admin får full information
                    var bookingDetails = new
                    {
                        Id = booking.Id,
                        Date = booking.Date.ToString("yyyy-MM-dd"),
                        Time = booking.Time.ToString(),
                        Customer = new
                        {
                            Id = booking.Customer.Id,
                            FirstName = booking.Customer.CustomerFirstName,
                            LastName = booking.Customer.CustomerLastName,
                            Address = address != null
                                ? new
                                {
                                    Id = address.Id,
                                    Street = address.CustomerStreetName,
                                    City = address.CustomerCity,
                                    PostalCode = address.CustomerPostalCode
                                }
                                : null
                        },
                        Cleaners = booking.BookingCleaners.Select(bc => new
                        {
                            Id = bc.Cleaner.Id,
                            FirstName = bc.Cleaner.EmployeeFirstName,
                            LastName = bc.Cleaner.EmployeeLastName,
                            Email = bc.Cleaner.EmployeeEmail,
                            Phone = bc.Cleaner.EmployeePhone
                        }).ToList()
                    };

                    return Ok(bookingDetails);
                }
                else
                {
                    // Kontrollera att städaren är kopplad till bokningen
                    bool hasAccess = await _context.BookingCleaner
                        .AnyAsync(bc => bc.BookingId == id && bc.CleanerId == employee.Id);

                    if (!hasAccess)
                        return Forbid("Du har inte behörighet att se denna bokning");

                    // Returnera begränsad information för städare
                    var cleanerBookingDetails = new
                    {
                        Id = booking.Id,
                        Date = booking.Date.ToString("yyyy-MM-dd"),
                        Time = booking.Time.ToString(),
                        Customer = new
                        {
                            Name = $"{booking.Customer.CustomerFirstName} {booking.Customer.CustomerLastName}",
                            Address = address != null
                                ? $"{address.CustomerStreetName}, {address.CustomerPostalCode} {address.CustomerCity}"
                                : "Adress saknas"
                        }
                    };

                    return Ok(cleanerBookingDetails);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internt fel: {ex.Message} - {ex.InnerException?.Message}");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ActionName("Skapa ny bokning")]
        public async Task<IActionResult> CreateBooking([FromBody] BookingCreateDto bookingDto)
        {
            if (bookingDto == null)
                return BadRequest("Bokning kan inte vara null");

            try
            {
                // Kontrollera att kunden finns
                var customer = await _context.Customers.FindAsync(bookingDto.CustomerId);
                if (customer == null)
                    return NotFound($"Kund med ID {bookingDto.CustomerId} hittades inte");

                // Skapa ny bokning
                var booking = new BookingEntity
                {
                    Customer = customer,
                    Date = bookingDto.Date,
                    Time = TimeOnly.Parse(bookingDto.Time) // Konvertera från string till TimeOnly
                };

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                // Om städare anges, koppla dem till bokningen
                if (bookingDto.CleanerIds != null && bookingDto.CleanerIds.Any())
                {
                    foreach (var cleanerId in bookingDto.CleanerIds)
                    {
                        var cleaner = await _context.Employees.FindAsync(cleanerId);
                        if (cleaner != null)
                        {
                            _context.BookingCleaner.Add(new BookingCleanerEntity
                            {
                                BookingId = booking.Id,
                                CleanerId = cleanerId
                            });
                        }
                        else
                        {
                            // Loggning
                            Console.WriteLine($"Varning: Städare med ID {cleanerId} hittades inte");
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                return CreatedAtAction(nameof(GetBookingById), new { id = booking.Id },
                    new { message = "Bokning skapad", bookingId = booking.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Fel vid skapande av bokning: {ex.Message} - {ex.InnerException?.Message}");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [ActionName("Uppdatera bokning")]
        public async Task<IActionResult> UpdateBooking(int id, [FromBody] BookingUpdateDto bookingDto)
        {
            if (bookingDto == null)
                return BadRequest("Bokning kan inte vara null");

            try
            {
                // Hämta existerande bokning med Include
                var booking = await _context.Bookings
                    .Include(b => b.Customer)
                    .FirstOrDefaultAsync(b => b.Id == id);

                if (booking == null)
                    return NotFound($"Ingen bokning hittades med id: {id}");

                // Uppdatera kund om en ny angetts
                if (bookingDto.CustomerId.HasValue)
                {
                    var customer = await _context.Customers.FindAsync(bookingDto.CustomerId.Value);
                    if (customer == null)
                        return NotFound($"Kund med ID {bookingDto.CustomerId} hittades inte");

                    booking.Customer = customer;
                }

                // Uppdatera datum och tid om nya angetts
                if (bookingDto.Date.HasValue)
                    booking.Date = bookingDto.Date.Value;

                if (!string.IsNullOrEmpty(bookingDto.Time))
                    booking.Time = TimeOnly.Parse(bookingDto.Time);

                await _context.SaveChangesAsync();

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
                        }
                    }

                    await _context.SaveChangesAsync();
                }

                return Ok(new { message = "Bokning uppdaterad", bookingId = id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Fel vid uppdatering av bokning: {ex.Message} - {ex.InnerException?.Message}");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ActionName("Radera bokning")]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            try
            {
                var booking = await _context.Bookings.FindAsync(id);
                if (booking == null)
                    return NotFound($"Ingen bokning hittades med id: {id}");

                // Ta bort kopplade städare först
                var bookingCleaners = await _context.BookingCleaner
                    .Where(bc => bc.BookingId == id)
                    .ToListAsync();

                foreach (var bc in bookingCleaners)
                {
                    _context.BookingCleaner.Remove(bc);
                }

                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Bokning raderad", bookingId = id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Fel vid radering av bokning: {ex.Message} - {ex.InnerException?.Message}");
            }
        }

        [HttpGet("customer/{customerId}")]
        [Authorize(Roles = "Admin")]
        [ActionName("Hämta kundens alla bokningar")]
        public async Task<IActionResult> GetCustomerBookings(int customerId)
        {
            try
            {
                // Kontrollera att kunden finns
                var customer = await _context.Customers.FindAsync(customerId);
                if (customer == null)
                    return NotFound($"Kund med ID {customerId} hittades inte");

                // Hämta kundens bokningar
                var bookings = await _context.Bookings
                    .Include(b => b.Customer)
                    .Include(b => b.BookingCleaners)
                        .ThenInclude(bc => bc.Cleaner)
                    .Where(b => b.Customer.Id == customerId)
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

                return Ok(customerBookings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internt fel: {ex.Message} - {ex.InnerException?.Message}");
            }
        }
    }

    // DTO för att skapa en bokning
    public class BookingCreateDto
    {
        public int CustomerId { get; set; }
        public DateTime Date { get; set; }

        // Använd string istället för TimeOnly för att undvika konverteringsproblem
        public string Time { get; set; } = "00:00:00";

        public List<int>? CleanerIds { get; set; }
    }

    // DTO för att uppdatera en bokning
    public class BookingUpdateDto
    {
        public int? CustomerId { get; set; }
        public DateTime? Date { get; set; }
        public string? Time { get; set; }
        public List<int>? CleanerIds { get; set; }
    }
}