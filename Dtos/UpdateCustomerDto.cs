namespace api_backend.Dtos
{
    public class UpdateCustomerDto
    {
        public string? CustomerFirstName { get; set; }
        public string? CustomerLastName { get; set; }
        public string? CustomerEmail {  get; set; }
        public string? CustomerAddress { get; set; }
        public string? CustomerCity { get; set; }
        public string? CustomerPostalCode { get; set; }
    }
}
