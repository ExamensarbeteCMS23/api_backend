using System.ComponentModel.DataAnnotations;

namespace api_backend.Models;

public class CleanerEntity
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string CleanerFirstName { get; set; } = null!;
    [Required]
    public string CleanerLastName { get; set; } = null!;
    [Required]
    public string CleanerEmail { get; set; } = null!;
    [Required]
    public string CleanerPhone { get; set; } = null!;
    [Required]
    public ICollection<BookingCleanerEntity> BookingCleaners { get; set; } = [];
    public int RoleId { get; set; }
    public RoleEntity Role { get; set; } = null!;
}
