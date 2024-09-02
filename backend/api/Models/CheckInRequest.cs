namespace api.Models
{
    public class CheckInRequest
    {
        public required Guid Id { get; set; }
        public required string GuestEmail { get; set; }
    }
}
