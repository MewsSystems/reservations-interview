using System.Text.RegularExpressions;

namespace Models.Validators
{
    public class RoomValidator
    {
        // Updated Regex Explanation:
        // ^         : Start of the string.
        // [0-9]     : The first digit must be a digit (0-9), representing the floor number.
        // [0-9]     : The second digit must be a digit (0-9), representing the first part of the door number.
        // [1-9]     : The third digit must be a non-zero digit (1-9), as '00' doors are invalid.
        // $         : End of the string.
        //
        // Valid Examples: "101", "202", "305" (valid 3-digit room numbers).
        // Invalid Examples: "-101" (negative), "000" ('00' doors are invalid), "2020" (too many digits).
        private static readonly Regex RoomNumberRegex = new Regex(@"^[0-9][0-9][1-9]$");

        /// <summary>
        /// Validates a room number based on the format and rules provided.
        /// </summary>
        /// <param name="roomNumber">The room number to validate.</param>
        /// <returns>True if valid, false otherwise.</returns>
        public bool IsValidRoomNumber(string roomNumber)
        {
            if (string.IsNullOrWhiteSpace(roomNumber))
            {
                return false;
            }

            return RoomNumberRegex.IsMatch(roomNumber);
        }
    }
}
