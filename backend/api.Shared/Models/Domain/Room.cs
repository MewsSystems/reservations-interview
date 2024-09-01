using api.Shared.Constants;
using api.Shared.Models.Errors;

namespace api.Shared.Models.Domain
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
    }
}
