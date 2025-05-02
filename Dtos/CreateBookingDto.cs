namespace api_backend.Dtos
{
    public class CreateBookingDto
    {
        public int CustomerId { get; set; }
        public DateTime Date { get; set; }
        public string Time { get; set; } = "00:00";
        public List<int>? CleanerIds { get; set; }
    }

}
