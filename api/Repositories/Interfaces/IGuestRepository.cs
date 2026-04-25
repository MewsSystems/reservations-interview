using Models;

namespace Repositories.Interfaces
{
    public interface IGuestRepository
    {
        Task<IEnumerable<Guest>> GetGuests();
        Task<Guest> GetGuestByEmail(string guestEmail);
        Task<Guest> CreateGuest(Guest newGuest);
        Task<bool> DeleteGuestByEmail(string guestEmail);
    }
}