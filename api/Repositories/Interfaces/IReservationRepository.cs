using Models;

namespace Repositories.Interfaces
{
    public interface IReservationRepository
    {
        Task<IEnumerable<Reservation>> GetReservations();
        Task<Reservation> GetReservation(Guid reservationId);
        Task<Reservation> CreateReservation(Reservation newReservation);
        Task<bool> HasConflict(Reservation reservation);
        Task<bool> DeleteReservation(Guid reservationId);
        Task<IEnumerable<Reservation>> GetUpcomingReservations();
        Task<bool> ExecuteCheckInTransaction(Guid reservationId, string roomNumber);
    }
}
