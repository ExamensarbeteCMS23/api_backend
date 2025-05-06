using api_backend.Dtos;

namespace api_backend.Interfaces;

public interface IBookingService
{
    Task<IEnumerable<object>> GetAllBookingsAsync();
    Task<object?> GetBookingByIdAsync(int id);
    Task<int> CreateBookingAsync(CreateBookingDto dto);
    Task<bool> DeleteBookingAsync(int id);
    Task<bool> UpdateBookingAsync(int id, BookingUpdateDto bookingDto);
    Task<IEnumerable<object>> GetCustomerBookingsAsync(int customerId);
    Task<IEnumerable<object>> GetCleanerBookingsAsync(int cleanerId);

}
