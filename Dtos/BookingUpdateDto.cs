namespace api_backend.Dtos
{
    public class BookingUpdateDto
    {
        public int? CustomerId { get; set; }
        public DateTime? Date { get; set; }
        public string? Time { get; set; }
        public List<int>? CleanerIds { get; set; }
    }
}
