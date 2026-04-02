using Models.Errors;

namespace Validators;

public static class RoomValidator
{
    public static void ValidateRoomNumber(string roomNumber)
    {
        if (string.IsNullOrWhiteSpace(roomNumber))
            throw new InvalidRoomNumber(roomNumber ?? "<null>");

        if (roomNumber.Length != 3)
            throw new InvalidRoomNumber(roomNumber);

        if (!roomNumber.All(char.IsDigit))
            throw new InvalidRoomNumber(roomNumber);

        if (roomNumber[1..] == "00")
            throw new InvalidRoomNumber(roomNumber);
    }
}