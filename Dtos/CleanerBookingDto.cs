namespace api_backend.Dtos;

public class CleanerBookingDto
{
    public int Id { get; set; }
    public string Date { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public CleanerCustomerDto Customer { get; set; } = new();
}