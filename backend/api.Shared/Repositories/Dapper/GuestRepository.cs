using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using api.Shared.Extensions;
using api.Shared.Models.DB;
using api.Shared.Models.Errors;
using Dapper;

namespace api.Shared.Repositories
{
    public class GuestRepository : IGuestRepository
    {
        private IDbConnection _db { get; set; }

        public GuestRepository(IDbConnection db)
        {
            _db = db.ThrowIfNull(nameof(IDbConnection));
        }

        public async Task<IEnumerable<Guest>> GetGuests()
        {
            var guests = await _db.QueryAsync<Guest>("SELECT * FROM Guests;");

            if (guests == null)
            {
                return [];
            }

            return guests;
        }

        public async Task<Guest> GetGuestByEmail(string guestEmail)
        {
            var guest = await _db.QueryFirstOrDefaultAsync<Guest>(
                "SELECT * FROM Guests WHERE Email = @guestEmail;",
                new { guestEmail }
            );

            if (guest == null)
            {
                throw new NotFoundException($"Guest {guestEmail} not found");
            }

            return guest;
        }

        public Task<Guest> CreateGuest(Guest newGuest)
        {
            return _db.QuerySingleAsync<Guest>(
                "INSERT INTO Guests(Email, Name) Values(@Email, @Name) RETURNING *",
                newGuest
            );
        }

        public async Task<bool> DeleteGuestByEmail(string guestEmail)
        {
            var count = await _db.ExecuteAsync(
                "DELETE FROM Guests WHERE Email = @guestEmail;",
                new { guestEmail }
            );

            return count > 0;
        }
    }
}
