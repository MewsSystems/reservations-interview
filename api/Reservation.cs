public class Reservation : NewReservation
{
    /// <summary>
    /// PKID for Reservations
    /// </summary>
    public Guid Id { get; set; }
}

public class NewReservation
{
    public int RoomNumber { get; set; }

    public required string GuestEmail { get; set; }

    public DateTime Start { get; set; }
    public DateTime End { get; set; }
}
