using Models;

namespace Validators;

public static class ReservationValidator
{
    public static void ValidateForCreate(Reservation reservation)
    {
        var startDate = reservation.Start.Date;
        var endDate = reservation.End.Date;
        
        if (endDate < startDate)
            throw new ArgumentException("End date must be after start date.");

        if (endDate == startDate)
            throw new ArgumentException("Reservation must be at least 1 day long.");

        var duration = endDate - startDate;

        if (duration.TotalDays > 30)
            throw new ArgumentException("Reservation cannot exceed 30 days.");
    }
}