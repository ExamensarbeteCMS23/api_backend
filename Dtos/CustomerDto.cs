namespace api_backend.Dtos
{
    public class CustomerDto
    {
        public int Id { get; set; }
        public string CustomerFirstName { get; set; } = null!;
        public string CustomerLastName { get; set; }= null!;
        public int AddressId { get; set; }
    }
}
