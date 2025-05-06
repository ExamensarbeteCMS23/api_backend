namespace api_backend.Dtos
{
    public class CreateCustomerRequestDto
    {
        // Customer fields
        public string CustomerFirstName { get; set; } = null!;
        public string CustomerLastName { get; set; } = null!;
        public string CustomerEmail { get; set; } = null!;

        // Address fields
        public string CustomerStreetName { get; set; } = null!;
        public string CustomerCity { get; set; } = null!;
        public string CustomerPostalCode { get; set; } = null!;
    }
}
