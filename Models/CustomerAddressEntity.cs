using System.ComponentModel.DataAnnotations;

namespace api_backend.Models;

public class CustomerAddressEntity
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string CustomerStreetName { get; set; } = null!;
    [Required]
    public string CustomerCity { get; set; } = null!;
    [Required]
    public string CustomerPostalCode { get; set; } = null!;
    public ICollection<CustomerEntity> Customers { get; set; } = [];
}
