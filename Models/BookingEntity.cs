using System.ComponentModel.DataAnnotations;

namespace api_backend.Models;

public class BookingEntity
{
    [Key]
    public int Id { get; set; }
    [Required]
    public CustomerEntity Customer { get; set; } = null!;
    public DateTime Date { get; set; }
    public TimeOnly Time { get; set; }
    public ICollection<BookingCleanerEntity> BookingCleaners { get; set; } = [];
}
