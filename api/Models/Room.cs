namespace Models
{
    /// <summary>
    /// Domain Model of a Room
    /// </summary>
    public class Room
    {
        /// <summary>
        /// PKID For Rooms. MewsHotel format is a three digit number with the first
        /// number being the floor number (up to 9) and the remaining two digits
        /// as the number of the door on the floor
        /// </summary>
        public required string Number { get; set; }

        /// <summary>
        /// Whether the room is available for reservation
        /// </summary>
        public State State { get; set; } = State.Ready;

        /// <summary>
        /// Whether the room needs cleaning
        /// </summary>
        public bool IsDirty { get; set; } = false;
    }

    public enum State
    {
        Ready = 0,
        Occupied = 1
    }

    public class RoomPatch
    {
        public bool? IsDirty { get; set; }
    }
}
