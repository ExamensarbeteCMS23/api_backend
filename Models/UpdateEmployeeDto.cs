namespace api_backend.Models
{
    public class UpdateEmployeeDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public int? RoleId { get; set; }
    }
}
