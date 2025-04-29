using System.ComponentModel.DataAnnotations;

namespace api_backend.Models;

public class EmployeeEntity
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string EmployeeFirstName { get; set; } = null!;
    [Required]
    public string EmployeeLastName { get; set; } = null!;
    [Required]
    public string EmployeeEmail { get; set; } = null!;
    [Required]
    public string EmployeePhone { get; set; } = null!;
    [Required]
    public ICollection<BookingCleanerEntity> BookingCleaners { get; set; } = [];
    public int RoleId { get; set; }
    public RoleEntity Role { get; set; } = null!;
}
