namespace api_backend.Dtos;

public class BookingDto
{
    public int Id { get; set; }
    public string Date { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public CustomerDto Customer { get; set; } = new();
}
