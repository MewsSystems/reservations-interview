namespace Services
{
    public interface ICheckInService
    {
        Task<(bool Success, string Error)> ProcessCheckIn(
            Guid reservationId,
            string emailConfirmation
        );
    }
}
