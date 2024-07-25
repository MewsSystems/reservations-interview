public class Room
{
    /// <summary>
    /// PKID For Rooms. MewsHotel format is a three digit number with the first
    /// number being the floor number (up to 9) and the remaining two digits
    /// as the number of the door on the floor
    /// </summary>
    public int Number { get; set; }

    /// <summary>
    /// Whether the room is available for reservation
    /// </summary>
    public State State { get; set; } = State.Ready;

    /// <summary>
    /// Formats the room number filling it with 0s
    /// to get a three digit string
    /// </summary>
    /// <returns></returns>
    public string FormatRoomNumber()
    {
        return Number.ToString().PadLeft(3, '0');
    }
}

public enum State
{
    Ready = 0,
    Occupied = 1,
    Dirty = 2
}
