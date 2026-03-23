using System.Text.RegularExpressions;
using Models.Errors;

namespace Models
{
    /// <summary>
    /// Domain Model of a Room
    /// </summary>
    public class Room
    {
        private static readonly Regex RoomNumberPattern = new(@"^[0-9]\d{2}$"); // allow ground floor 0

        /// <summary>
        /// PKID For Rooms. Format is "###" where first digit is floor (1-9)
        /// and last two digits are the door number (01-99).
        /// </summary>
        public required string Number { get; set; }

        /// <summary>
        /// Whether the room is available for reservation
        /// </summary>
        public State State { get; set; } = State.Ready;

        /// <summary>
        /// Validates the room number format. Must be 3 digits, first digit 1-9, last two not "00".
        /// </summary>
        public static bool IsValidRoomNumber(string roomNumber)
        {
            return RoomNumberPattern.IsMatch(roomNumber) && roomNumber[1..] != "00";
        }

        public static void ValidateRoomNumber(string roomNumber)
        {
            if (!IsValidRoomNumber(roomNumber))
            {
                throw new ValidationException(nameof(Room), roomNumber, $"The value {roomNumber} is not a valid room number");
            }
        }
    }

    public enum State
    {
        Ready = 0,
        Occupied = 1,
        Dirty = 2
    }
}
