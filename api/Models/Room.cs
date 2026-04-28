using System.Text.RegularExpressions;
using Models.Errors;

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
        /// Formats the room number filling it with 0s
        /// to get a three digit string
        /// </summary>
        /// <returns></returns>
        public static string FormatRoomNumber(int number)
        {
            return number.ToString().PadLeft(3, '0');
        }

        /// <summary>
        /// Validates that a room number string matches the "###" format:
        /// - Exactly three digits
        /// - Door portion (last two digits) cannot be "00"
        /// </summary>
        public static bool IsValidRoomNumber(string roomNumber)
        {
            return Regex.IsMatch(roomNumber, @"^\d{3}$") && !roomNumber.EndsWith("00");
        }

        public static int ConvertRoomNumberToInt(string roomNumber)
        {
            var success = int.TryParse(roomNumber, out int roomNumberInt);
            if (!success)
            {
                throw new InvalidRoomNumber(roomNumber);
            }

            return roomNumberInt;
        }
    }

    public enum State
    {
        Ready = 0,
        Occupied = 1,
        Dirty = 2
    }
}
