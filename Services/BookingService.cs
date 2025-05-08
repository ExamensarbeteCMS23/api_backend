using api_backend.Dtos;
using api_backend.Interfaces;
using api_backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace api_backend.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly ILogger<BookingService> _logger;

        public BookingService(IBookingRepository bookingRepository, ILogger<BookingService> logger)
        {
            _bookingRepository = bookingRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<object>> GetAllBookingsAsync()
        {
            try
            {
                _logger.LogInformation("BookingService.cs i användning");
                _logger.LogInformation("Service: GetAllBookingsAsync startar");

                var bookings = await _bookingRepository.GetAllWithCustomersAsync();

                _logger.LogInformation($"Service: Fick {bookings.Count()} bokningar från repository");

                return bookings.Select(b => new
                {
                    Id = b.Id,
                    Date = b.Date.ToString("yyyy-MM-dd"),
                    Time = b.Time.ToString(),
                    Customer = new
                    {
                        Id = b.Customer.Id,
                        Name = $"{b.Customer.CustomerFirstName} {b.Customer.CustomerLastName}"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Service: Error retrieving all bookings");
                throw;
            }
        }

        public async Task<object?> GetBookingByIdAsync(int id)
        {
            try
            {
                var booking = await _bookingRepository.GetByIdWithCustomerAsync(id);

                if (booking == null)
                    return null;

                return new
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving booking with ID {BookingId}", id);
                throw;
            }
        }

        public async Task<int> CreateBookingAsync(CreateBookingDto dto)
        {
            try
            {
                // Validera att kunden finns
                var customerExists = await _bookingRepository.CustomerExistsAsync(dto.CustomerId);
                if (!customerExists)
                {
                    throw new NotFoundException($"Customer with ID {dto.CustomerId} not found");
                }

                // Parse time
                if (!TimeOnly.TryParse(dto.Time, out TimeOnly parsedTime))
                {
                    throw new ArgumentException($"Invalid time format: {dto.Time}. Use HH:mm or HH:mm:ss format");
                }

                // Skapa bokning
                var booking = new BookingEntity
                {
                    CustomerId = dto.CustomerId,
                    Date = dto.Date,
                    Time = parsedTime
                };

                var bookingId = await _bookingRepository.CreateAsync(booking);

                // Lägg till städare om angivna
                if (dto.CleanerIds != null && dto.CleanerIds.Any())
                {
                    foreach (var cleanerId in dto.CleanerIds)
                    {
                        var cleanerExists = await _bookingRepository.CleanerExistsAsync(cleanerId);
                        if (cleanerExists)
                        {
                            await _bookingRepository.AddBookingCleanerAsync(bookingId, cleanerId);
                        }
                        else
                        {
                            _logger.LogWarning("Cleaner with ID {CleanerId} not found", cleanerId);
                        }
                    }
                }

                return bookingId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating booking");
                throw;
            }
        }

        public async Task<bool> DeleteBookingAsync(int id)
        {
            try
            {
                // Ta bort städarkopplingar först
                var bookingCleaners = await _bookingRepository.GetBookingCleanersByBookingIdAsync(id);
                if (bookingCleaners.Any())
                {
                    await _bookingRepository.RemoveBookingCleanersAsync(bookingCleaners);
                }

                // Ta bort bokningen
                return await _bookingRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting booking with ID {BookingId}", id);
                throw;
            }
        }

        public async Task<bool> UpdateBookingAsync(int id, BookingUpdateDto dto)
        {
            try
            {
                var booking = await _bookingRepository.GetByIdWithCustomerAsync(id);
                if (booking == null)
                    return false;

                // Uppdatera kund om angiven
                if (dto.CustomerId.HasValue)
                {
                    var customerExists = await _bookingRepository.CustomerExistsAsync(dto.CustomerId.Value);
                    if (!customerExists)
                    {
                        throw new NotFoundException($"Customer with ID {dto.CustomerId} not found");
                    }
                    booking.CustomerId = dto.CustomerId.Value;
                }

                // Uppdatera datum om angivet
                if (dto.Date.HasValue)
                    booking.Date = dto.Date.Value;

                // Uppdatera tid om angiven
                if (!string.IsNullOrEmpty(dto.Time))
                {
                    if (!TimeOnly.TryParse(dto.Time, out TimeOnly parsedTime))
                    {
                        throw new ArgumentException($"Invalid time format: {dto.Time}. Use HH:mm or HH:mm:ss format");
                    }
                    booking.Time = parsedTime;
                }

                await _bookingRepository.UpdateAsync(booking);

                // Uppdatera städarkopplingar om angivna
                if (dto.CleanerIds != null)
                {
                    // Ta bort befintliga kopplingar
                    var existingBookingCleaners = await _bookingRepository.GetBookingCleanersByBookingIdAsync(id);
                    await _bookingRepository.RemoveBookingCleanersAsync(existingBookingCleaners);

                    // Lägg till nya kopplingar
                    foreach (var cleanerId in dto.CleanerIds)
                    {
                        var cleanerExists = await _bookingRepository.CleanerExistsAsync(cleanerId);
                        if (cleanerExists)
                        {
                            await _bookingRepository.AddBookingCleanerAsync(id, cleanerId);
                        }
                        else
                        {
                            _logger.LogWarning("Cleaner with ID {CleanerId} not found", cleanerId);
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating booking with ID {BookingId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<object>> GetCustomerBookingsAsync(int customerId)
        {
            try
            {
                var customerExists = await _bookingRepository.CustomerExistsAsync(customerId);
                if (!customerExists)
                {
                    throw new NotFoundException($"Customer with ID {customerId} not found");
                }

                var bookings = await _bookingRepository.GetByCustomerIdWithCleanersAsync(customerId);

                return bookings.Select(b => new
                {
                    Id = b.Id,
                    Date = b.Date.ToString("yyyy-MM-dd"),
                    Time = b.Time.ToString(),
                    Cleaners = b.BookingCleaners.Select(bc => new
                    {
                        Id = bc.Cleaner.Id,
                        Name = $"{bc.Cleaner.EmployeeFirstName} {bc.Cleaner.EmployeeLastName}"
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving bookings for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<IEnumerable<object>> GetCleanerBookingsAsync(int cleanerId)
        {
            try
            {
                var cleanerExists = await _bookingRepository.CleanerExistsAsync(cleanerId);
                if (!cleanerExists)
                {
                    throw new NotFoundException($"Cleaner with ID {cleanerId} not found");
                }

                var bookings = await _bookingRepository.GetByCleanerIdWithCustomerAndAddressAsync(cleanerId);

                return bookings.Select(b => new
                {
                    Id = b.Id,
                    Date = b.Date.ToString("yyyy-MM-dd"),
                    Time = b.Time.ToString(),
                    Customer = new
                    {
                        Name = $"{b.Customer.CustomerFirstName} {b.Customer.CustomerLastName}"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving bookings for cleaner {CleanerId}", cleanerId);
                throw;
            }
        }
        public async Task<IEnumerable<object>> GetMyBookingsAsync(int employeeId)
        {
            try
            {
                _logger.LogInformation("Service: GetMyBookingsAsync startar för anställd {EmployeeId}", employeeId);

                var cleanerExists = await _bookingRepository.CleanerExistsAsync(employeeId);
                if (!cleanerExists)
                {
                    throw new NotFoundException($"Cleaner with ID {employeeId} not found");
                }

                var bookings = await _bookingRepository.GetByCleanerIdWithCustomerAndAddressAsync(employeeId);

                // Hämta kundens information utan att använda Address-egenskapen
                var formattedBookings = bookings.Select(b => new
                {
                    Id = b.Id,
                    Date = b.Date.ToString("yyyy-MM-dd"),
                    Time = b.Time.ToString(),
                    Customer = new
                    {
                        Name = $"{b.Customer.CustomerFirstName} {b.Customer.CustomerLastName}",
                        // Vi undviker att använda b.Customer.Address här
                    }
                });

                _logger.LogInformation("Retrieved {Count} bookings for cleaner {CleanerId}", bookings.Count(), employeeId);
                return formattedBookings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving bookings for cleaner {CleanerId}", employeeId);
                throw;
            }
        }
    }

    // Custom exception classes
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }
}