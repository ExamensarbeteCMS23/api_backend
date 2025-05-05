using api_backend.Contexts;
using api_backend.Interfaces;
using api_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace api_backend.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly DataContext _context;

        public BookingRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BookingEntity>> GetAllWithCustomersAsync()
        {
            return await _context.Bookings
                .Include(b => b.Customer)
                .ToListAsync();
        }

        public async Task<BookingEntity?> GetByIdWithCustomerAsync(int id)
        {
            return await _context.Bookings
                .Include(b => b.Customer)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<int> CreateAsync(BookingEntity booking)
        {
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
            return booking.Id;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
                return false;

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateAsync(BookingEntity booking)
        {
            _context.Bookings.Update(booking);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<IEnumerable<BookingEntity>> GetByCustomerIdWithCleanersAsync(int customerId)
        {
            return await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.BookingCleaners)
                    .ThenInclude(bc => bc.Cleaner)
                .Where(b => b.CustomerId == customerId)
                .ToListAsync();
        }

        public async Task<IEnumerable<BookingEntity>> GetByCleanerIdWithCustomerAndAddressAsync(int cleanerId)
        {
            var bookingIds = await GetBookingIdsByCleanerIdAsync(cleanerId);

            return await _context.Bookings
                .Include(b => b.Customer)
                .Where(b => bookingIds.Contains(b.Id))
                .ToListAsync();
        }

        public async Task<IList<int>> GetBookingIdsByCleanerIdAsync(int cleanerId)
        {
            return await _context.BookingCleaner
                .Where(bc => bc.CleanerId == cleanerId)
                .Select(bc => bc.BookingId)
                .ToListAsync();
        }

        public async Task<bool> CustomerExistsAsync(int customerId)
        {
            return await _context.Customers.AnyAsync(c => c.Id == customerId);
        }

        public async Task<bool> CleanerExistsAsync(int cleanerId)
        {
            return await _context.Employees.AnyAsync(e => e.Id == cleanerId);
        }

        public async Task AddBookingCleanerAsync(int bookingId, int cleanerId)
        {
            var bookingCleaner = new BookingCleanerEntity
            {
                BookingId = bookingId,
                CleanerId = cleanerId
            };
            _context.BookingCleaner.Add(bookingCleaner);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveBookingCleanerAsync(int bookingId, int cleanerId)
        {
            var bookingCleaner = await _context.BookingCleaner
                .FirstOrDefaultAsync(bc => bc.BookingId == bookingId && bc.CleanerId == cleanerId);

            if (bookingCleaner != null)
            {
                _context.BookingCleaner.Remove(bookingCleaner);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<BookingCleanerEntity>> GetBookingCleanersByBookingIdAsync(int bookingId)
        {
            return await _context.BookingCleaner
                .Where(bc => bc.BookingId == bookingId)
                .ToListAsync();
        }

        public async Task RemoveBookingCleanersAsync(IEnumerable<BookingCleanerEntity> bookingCleaners)
        {
            _context.BookingCleaner.RemoveRange(bookingCleaners);
            await _context.SaveChangesAsync();
        }
    }
}