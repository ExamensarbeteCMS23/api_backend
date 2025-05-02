using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api_backend.Models;

public class BookingEntity
{
    [Key]
    public int Id { get; set; }

    // Add the foreign key property
    [Required]
    public int CustomerId { get; set; }

    // Navigation property
    [ForeignKey("CustomerId")]
    public CustomerEntity Customer { get; set; } = null!;

    public DateTime Date { get; set; }
    public TimeOnly Time { get; set; }

    public ICollection<BookingCleanerEntity> BookingCleaners { get; set; } = [];
}