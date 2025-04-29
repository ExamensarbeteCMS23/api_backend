using Microsoft.AspNetCore.Identity;

namespace api_backend.Models
{
    public class ApplicationUser : IdentityUser
    {
        public int EmployeeId { get; set; }
        public EmployeeEntity Employee { get; set; }
    }
}
