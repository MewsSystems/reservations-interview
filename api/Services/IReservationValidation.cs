using System.Text.RegularExpressions;
using Models;

namespace api.Services;

public interface IReservationValidation
{
    string ValidateReservation(Reservation room);
    string ValidateRoomNumber(string roomNumber);
}

public partial class ReservationValidation : IReservationValidation
{
    public string ValidateReservation(Reservation room)
    {
        if (room.Start == default)
            return "Start Date is required.";
        if (room.End == default)
            return "End Date is required.";
        if (string.IsNullOrEmpty(room.GuestEmail))
            return "Email is required.";
        if (string.IsNullOrEmpty(room.RoomNumber))
            return "Room Number is required.";

        if (room.Start >= room.End)
            return "Start Date must be before the End Date.";

        var duration = (room.End - room.Start).TotalDays;
        if (duration < 1)
            return "Duration must be at least 1 day.";
        if (duration > 30)
            return "Duration cannot exceed 30 days.";

        if (!EmailValidationRegex().IsMatch(room.GuestEmail))
            return "Invalid email format. Email must include a domain.";

        var validateRoomNumber = ValidateRoomNumber(room.RoomNumber);
        if (!string.IsNullOrEmpty(validateRoomNumber))
            return validateRoomNumber;

        return string.Empty;
    }

    public string ValidateRoomNumber(string roomNumber)
    {
        if (!Regex.IsMatch(roomNumber, @"^\d{3}$"))
            return "Room number must be exactly 3 digits";

        // Parse the room number digits
        var floor = int.Parse(roomNumber[0].ToString());
        var door = int.Parse(roomNumber.Substring(1, 2));

        if (floor < 0 || floor > 9)
            return "Floor number must be between 0 and 9";

        if (door == 0)
            return "Door number cannot be 00";

        return string.Empty;
    }

    // Taken from https://emailregex.com/
    [GeneratedRegex(@"(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|""(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*"")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\[(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-z0-9-]*[a-z0-9]:(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21-\x5a\x53-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])+)\])")]
    private static partial Regex EmailValidationRegex();
}
