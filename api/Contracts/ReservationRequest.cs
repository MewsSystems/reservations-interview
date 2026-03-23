namespace Contracts
{
    public class ReservationRequest
    {
        public required string RoomNumber { get; set; }
        public required string GuestEmail { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}
