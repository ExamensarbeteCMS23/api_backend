using System.ComponentModel.DataAnnotations;

namespace api_backend.Models;

public class CustomerEntity
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string CustomerFirstName { get; set; } = null!;
    [Required]
    public string CustomerLastName { get; set; } = null!;
    public int AddressId { get; set; }
}
