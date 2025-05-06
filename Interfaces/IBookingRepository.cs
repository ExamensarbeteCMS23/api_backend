using api_backend.Models;

namespace api_backend.Interfaces;

public interface IBookingRepository
{
    Task<int> CreateAsync(BookingEntity booking);
    Task<bool> DeleteAsync(int id);
    Task<bool> UpdateAsync(BookingEntity booking);
    Task<BookingEntity?> GetByIdWithCustomerAsync(int id);
    Task<IEnumerable<BookingEntity>> GetAllWithCustomersAsync();
    Task<IEnumerable<BookingEntity>> GetByCustomerIdWithCleanersAsync(int customerId);
    Task<IEnumerable<BookingEntity>> GetByCleanerIdWithCustomerAndAddressAsync(int cleanerId);
    Task<IList<int>> GetBookingIdsByCleanerIdAsync(int cleanerId);
    Task<bool> CustomerExistsAsync(int customerId);
    Task<bool> CleanerExistsAsync(int cleanerId);
    Task AddBookingCleanerAsync(int bookingId, int cleanerId);
    Task RemoveBookingCleanerAsync(int bookingId, int cleanerId);
    Task<IEnumerable<BookingCleanerEntity>> GetBookingCleanersByBookingIdAsync(int bookingId);
    Task RemoveBookingCleanersAsync(IEnumerable<BookingCleanerEntity> bookingCleaners);
}
