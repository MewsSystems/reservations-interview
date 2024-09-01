using api.Shared.Models.DB;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace api.Shared.Repositories
{
    public interface IGuestRepository
    {
        Task<IEnumerable<Guest>> GetGuests();

        Task<Guest> GetGuestByEmail(string guestEmail);

        Task<Guest> CreateGuest(Guest newGuest, IDbTransaction? transaction = null);

        Task<bool> DeleteGuestByEmail(string guestEmail);
    }
}