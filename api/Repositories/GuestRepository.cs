using System.Data;
using Dapper;
using Models;
using Models.Errors;

namespace Repositories
{
    public class GuestRepository
    {
        private IDbConnection _db { get; set; }

        public GuestRepository(IDbConnection db)
        {
            _db = db;
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
