using System.ComponentModel.DataAnnotations;

namespace api_backend.Models;

public class RoleEntity
{
    [Key]
    public int Id { get; set; }
    [Required]
    public String Role { get; set; } = null!;
    public ICollection<CleanerEntity> Cleaners { get; set; } = [];
}
