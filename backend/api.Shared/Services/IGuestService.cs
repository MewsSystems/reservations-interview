using api.Shared.Models.Domain;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace api.Shared.Services
{
    public interface IGuestService
    {
        Task<IEnumerable<Guest>> Get();

        Task<Guest> GetByEmail(string guestEmail);

        Task<Guest> Create(Guest newGuest, IDbTransaction? transaction = null);
    }
}