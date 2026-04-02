using Models;
using Models.Errors;

namespace Extensions
{
    public static class RoomExtensions
    {
        /// <summary>
        /// Formats the room number filling it with 0s
        /// to get a three digit string
        /// </summary>
        public static string FormatRoomNumber(int number)
        {
            return number.ToString().PadLeft(3, '0');
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

        /// <summary>
        /// Validates a room against RE-001 rules.
        /// Returns a list of validation errors (empty if valid).
        /// </summary>
        public static List<string> Validate(this Room room)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(room.Number))
            {
                errors.Add("Room number is required.");
                return errors;
            }

            if (room.Number.Length != 3)
            {
                errors.Add("Room number must be exactly 3 digits in the format \"###\".");
                return errors;
            }

            if (!room.Number.All(char.IsDigit))
            {
                errors.Add("Room number must contain only digits 0-9.");
                return errors;
            }

            var door = room.Number.Substring(1, 2);
            if (door == "00")
            {
                errors.Add("Door number cannot be \"00\".");
            }

            return errors;
        }
    }
}
