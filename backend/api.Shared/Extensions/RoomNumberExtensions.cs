using api.Shared.Models.Errors;

namespace api.Shared.Extensions
{
    public static class RoomNumberExtensions
    {
        /// <summary>
        /// Formats the room number filling it with 0s
        /// to get a three digit string
        /// </summary>
        /// <returns></returns>
        public static string FormatRoomNumber(this int number)
        {
            return number.ToString().PadLeft(3, '0');
        }

        public static int ConvertRoomNumberToInt(this string roomNumber)
        {
            var success = int.TryParse(roomNumber, out int roomNumberInt);
            if (!success)
            {
                throw new InvalidRoomNumber(roomNumber);
            }
            return roomNumberInt;
        }
    }
}
