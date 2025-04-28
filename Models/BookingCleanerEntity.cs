namespace api_backend.Models;

public class BookingCleanerEntity
{
    public int BookingId { get; set; }
    public BookingEntity Booking { get; set; } = null!;
    public int CleanerId { get; set; }
    public CleanerEntity Cleaner { get; set; } = null!;
}
