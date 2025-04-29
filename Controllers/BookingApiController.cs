using api_backend.Contexts;
using api_backend.Models;
using api_backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace api_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingApiController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly FakeAuthService _authService;

        public BookingApiController(DataContext context, FakeAuthService authService)
        {
            _context = context;
            _authService = authService;
        }
        //	Skapar testdata: adress, kund, städare, roll, bokning. Endast Admin.
        [HttpGet]
        [Route("seed")]
        [ActionName("Seed testdata")]
        public IActionResult SeedTestData()
        {
            // Endast admin kan köra seed
            if (!_authService.IsAdmin())
                return Forbid("Endast administratörer kan köra seed-data");

            if (_context.Bookings.Any())
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
                _context.SaveChanges();

                // 2. Skapa kund
                var customer = new CustomerEntity
                {
                    CustomerFirstName = "Tina",
                    CustomerLastName = "Lutti",
                    AddressId = address.Id
                };
                _context.Customers.Add(customer);
                _context.SaveChanges();

                // 3. Skapa roll (om det inte redan finns)
                var role = _context.Roles.FirstOrDefault(r => r.Role == "Cleaner");
                if (role == null)
                {
                    role = new RoleEntity { Role = "Cleaner" };
                    _context.Roles.Add(role);
                    _context.SaveChanges();
                }

                // 4. Skapa cleaner
                var cleaner = new CleanerEntity
                {
                    CleanerFirstName = "Anna",
                    CleanerLastName = "Städsson",
                    CleanerEmail = "anna@clean.com",
                    CleanerPhone = "0701234567",
                    RoleId = role.Id
                };
                _context.Cleaners.Add(cleaner);
                _context.SaveChanges();

                // 5. Skapa bokning
                var booking = new BookingEntity
                {
                    Customer = customer,
                    Date = DateTime.Today.AddDays(1),
                    Time = new TimeOnly(9, 0)
                };
                _context.Bookings.Add(booking);
                _context.SaveChanges();

                // 6. Skapa koppling mellan bokning och städare
                var bookingCleaner = new BookingCleanerEntity
                {
                    BookingId = booking.Id,
                    CleanerId = cleaner.Id
                };
                _context.BookingCleaner.Add(bookingCleaner);
                _context.SaveChanges();

                return Ok($"Testdata skapad! Städare ID: {cleaner.Id}, Bokning ID: {booking.Id}, Kund ID: {customer.Id}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Fel vid skapande av testdata: {ex.Message} - {ex.InnerException?.Message}");
            }
        }

        // GET: api/BookingApi 	Hämtar alla bokningar: Admin ser alla bokningar + alla detaljer. Cleaner ser endast sina egna.

        [HttpGet]
        [ActionName("GetAllBookings")]
        [Route("")]
        public IActionResult GetAllBookings()
        {
            try
            {
                if (_authService.IsAdmin())
                {
                    // Admin får se alla bokningar med full information
                    var bookings = _context.Bookings
                        .Include(b => b.Customer)
                        .Include(b => b.BookingCleaners)
                            .ThenInclude(bc => bc.Cleaner)
                        .ToList();

                    // Hämta också adressinformation för varje kund
                    var customerIds = bookings.Select(b => b.Customer.Id).ToList();
                    var addressInfo = _context.Customers
                        .Where(c => customerIds.Contains(c.Id))
                        .Join(_context.CustomerAddresses,
                            c => c.AddressId,
                            a => a.Id,
                            (c, a) => new { CustomerId = c.Id, Address = a })
                        .ToDictionary(x => x.CustomerId, x => x.Address);

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
                            Name = $"{bc.Cleaner.CleanerFirstName} {bc.Cleaner.CleanerLastName}",
                            Email = bc.Cleaner.CleanerEmail,
                            Phone = bc.Cleaner.CleanerPhone
                        }).ToList()
                    }).ToList();

                    return Ok(bookingsDetails);
                }
                else if (_authService.IsCleaner())
                {
                    // Städare får bara se sina egna bokningar
                    int cleanerId = _authService.GetCurrentCleanerId();

                    var bookingIds = _context.BookingCleaner
                        .Where(bc => bc.CleanerId == cleanerId)
                        .Select(bc => bc.BookingId)
                        .ToList();

                    if (!bookingIds.Any())
                        return Ok(new List<object>()); // Returnera tom lista om inga bokningar finns

                    var bookings = _context.Bookings
                        .Include(b => b.Customer)
                        .Where(b => bookingIds.Contains(b.Id))
                        .ToList();

                    // Hämta adressinformation
                    var customerIds = bookings.Select(b => b.Customer.Id).ToList();
                    var addressInfo = _context.Customers
                        .Where(c => customerIds.Contains(c.Id))
                        .Join(_context.CustomerAddresses,
                            c => c.AddressId,
                            a => a.Id,
                            (c, a) => new { CustomerId = c.Id, Address = a })
                        .ToDictionary(x => x.CustomerId, x => x.Address);

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

                return Forbid("Din roll har inte behörighet att se bokningar");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internt fel: {ex.Message} - {ex.InnerException?.Message}");
            }
        }

        // GET: api/BookingApi/5
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            try
            {
                var booking = _context.Bookings
                    .Include(b => b.Customer)
                    .Include(b => b.BookingCleaners)
                        .ThenInclude(bc => bc.Cleaner)
                    .FirstOrDefault(b => b.Id == id);

                if (booking == null)
                    return NotFound($"Ingen bokning hittades med id: {id}");

                // Hämta adressinformation
                var address = _context.CustomerAddresses
                    .FirstOrDefault(a => a.Id == booking.Customer.AddressId);

                if (_authService.IsAdmin())
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
                            FirstName = bc.Cleaner.CleanerFirstName,
                            LastName = bc.Cleaner.CleanerLastName,
                            Email = bc.Cleaner.CleanerEmail,
                            Phone = bc.Cleaner.CleanerPhone
                        }).ToList()
                    };

                    return Ok(bookingDetails);
                }
                else if (_authService.IsCleaner())
                {
                    int cleanerId = _authService.GetCurrentCleanerId();

                    // Kontrollera att städaren är kopplad till bokningen
                    bool hasAccess = _context.BookingCleaner
                        .Any(bc => bc.BookingId == id && bc.CleanerId == cleanerId);

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

                return Forbid("Din roll har inte behörighet att se bokningar");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internt fel: {ex.Message} - {ex.InnerException?.Message}");
            }
        }

        // POST: api/BookingApi
        [HttpPost]
        public IActionResult CreateBooking([FromBody] BookingCreateDto bookingDto)
        {
            // Endast admin kan skapa bokningar
            if (!_authService.IsAdmin())
                return Forbid("Endast administratörer kan skapa bokningar");

            if (bookingDto == null)
                return BadRequest("Bokning kan inte vara null");

            try
            {
                // Kontrollera att kunden finns
                var customer = _context.Customers.Find(bookingDto.CustomerId);
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
                _context.SaveChanges();

                // Om städare anges, koppla dem till bokningen
                if (bookingDto.CleanerIds != null && bookingDto.CleanerIds.Any())
                {
                    foreach (var cleanerId in bookingDto.CleanerIds)
                    {
                        var cleaner = _context.Cleaners.Find(cleanerId);
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
                    _context.SaveChanges();
                }

                return CreatedAtAction(nameof(GetById), new { id = booking.Id },
                    new { message = "Bokning skapad", bookingId = booking.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Fel vid skapande av bokning: {ex.Message} - {ex.InnerException?.Message}");
            }
        }

        // PUT: api/BookingApi/5
        [HttpPut("{id}")]
        public IActionResult UpdateBooking(int id, [FromBody] BookingUpdateDto bookingDto)
        {
            // Endast admin kan uppdatera bokningar
            if (!_authService.IsAdmin())
                return Forbid("Endast administratörer kan uppdatera bokningar");

            if (bookingDto == null)
                return BadRequest("Bokning kan inte vara null");

            try
            {
                // Hämta existerande bokning
                var booking = _context.Bookings.Find(id);
                if (booking == null)
                    return NotFound($"Ingen bokning hittades med id: {id}");

                // Uppdatera kund om en ny angetts
                if (bookingDto.CustomerId.HasValue)
                {
                    var customer = _context.Customers.Find(bookingDto.CustomerId.Value);
                    if (customer == null)
                        return NotFound($"Kund med ID {bookingDto.CustomerId} hittades inte");

                    booking.Customer = customer;
                }

                // Uppdatera datum och tid om nya angetts
                if (bookingDto.Date.HasValue)
                    booking.Date = bookingDto.Date.Value;

                if (!string.IsNullOrEmpty(bookingDto.Time))
                    booking.Time = TimeOnly.Parse(bookingDto.Time);

                _context.SaveChanges();

                // Uppdatera städarkopplingar om nya angetts
                if (bookingDto.CleanerIds != null)
                {
                    // Ta bort befintliga kopplingar
                    var existingBookingCleaners = _context.BookingCleaner
                        .Where(bc => bc.BookingId == id)
                        .ToList();

                    foreach (var bc in existingBookingCleaners)
                    {
                        _context.BookingCleaner.Remove(bc);
                    }

                    // Lägg till nya kopplingar
                    foreach (var cleanerId in bookingDto.CleanerIds)
                    {
                        var cleaner = _context.Cleaners.Find(cleanerId);
                        if (cleaner != null)
                        {
                            _context.BookingCleaner.Add(new BookingCleanerEntity
                            {
                                BookingId = id,
                                CleanerId = cleanerId
                            });
                        }
                    }

                    _context.SaveChanges();
                }

                return Ok(new { message = "Bokning uppdaterad", bookingId = id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Fel vid uppdatering av bokning: {ex.Message} - {ex.InnerException?.Message}");
            }
        }

        // DELETE: api/BookingApi/5
        [HttpDelete("{id}")]
        public IActionResult DeleteBooking(int id)
        {
            // Endast admin kan radera bokningar
            if (!_authService.IsAdmin())
                return Forbid("Endast administratörer kan radera bokningar");

            try
            {
                var booking = _context.Bookings.Find(id);
                if (booking == null)
                    return NotFound($"Ingen bokning hittades med id: {id}");

                // Ta bort kopplade städare först
                var bookingCleaners = _context.BookingCleaner
                    .Where(bc => bc.BookingId == id)
                    .ToList();

                foreach (var bc in bookingCleaners)
                {
                    _context.BookingCleaner.Remove(bc);
                }

                _context.Bookings.Remove(booking);
                _context.SaveChanges();

                return Ok(new { message = "Bokning raderad", bookingId = id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Fel vid radering av bokning: {ex.Message} - {ex.InnerException?.Message}");
            }
        }

        // GET: api/BookingApi/customer/5
        [HttpGet("customer/{customerId}")]
        public IActionResult GetBookingsByCustomer(int customerId)
        {
            // Endast admin kan se alla kundens bokningar
            if (!_authService.IsAdmin())
                return Forbid("Endast administratörer kan se alla kundens bokningar");

            try
            {
                // Kontrollera att kunden finns
                var customer = _context.Customers.Find(customerId);
                if (customer == null)
                    return NotFound($"Kund med ID {customerId} hittades inte");

                // Hämta kundens bokningar
                var bookings = _context.Bookings
                    .Include(b => b.Customer)
                    .Include(b => b.BookingCleaners)
                        .ThenInclude(bc => bc.Cleaner)
                    .Where(b => b.Customer.Id == customerId)
                    .ToList();

                // Formatera svaret
                var customerBookings = bookings.Select(b => new
                {
                    Id = b.Id,
                    Date = b.Date.ToString("yyyy-MM-dd"),
                    Time = b.Time.ToString(),
                    Cleaners = b.BookingCleaners.Select(bc => new
                    {
                        Id = bc.Cleaner.Id,
                        Name = $"{bc.Cleaner.CleanerFirstName} {bc.Cleaner.CleanerLastName}"
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