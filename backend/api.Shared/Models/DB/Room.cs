using api.Shared.Constants;
using api.Shared.Models.Domain;

namespace api.Shared.Models.DB
{
    // Inner class to hide the details of a direct mapping to SQLite
    public class Room
    {
        /// <summary>
        /// PKID For Rooms. SQLite stores as an integer
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// Whether the room is available for reservation
        /// </summary>
        public State State { get; set; } = State.Ready;
    }
}
