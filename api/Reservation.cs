public class Reservation : NewReservation
{
    /// <summary>
    /// PKID for Reservations
    /// </summary>
    public Guid Id { get; set; }

    public DbReservation ToDb()
    {
        return new DbReservation
        {
            Id = Id.ToString(),
            RoomNumber = RoomNumber,
            GuestEmail = GuestEmail,
            Start = Start,
            End = End
        };
    }
}

/// <summary>
/// A reservation's data but without its persistence ID (ie new one from the FE)
/// </summary>
public class NewReservation
{
    public int RoomNumber { get; set; }

    public required string GuestEmail { get; set; }

    public DateTime Start { get; set; }
    public DateTime End { get; set; }
}

/// <summary>
/// A DB safe class to serialize to, as SQLite uses Text instead of GUID
/// </summary>
public class DbReservation : NewReservation
{
    public required string Id { get; set; }

    public Reservation ToDomain()
    {
        return new Reservation
        {
            Id = Guid.Parse(Id),
            GuestEmail = GuestEmail,
            Start = Start,
            End = End
        };
    }
}
