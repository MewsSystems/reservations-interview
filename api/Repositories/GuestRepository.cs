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

        public async Task<bool> GuestExists(string guestEmail)
        {
            var count = await _db.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM Guests WHERE Email = @guestEmail;",
                new { guestEmail }
            );

            return count > 0;
        }

        public async Task<Guest> CreateGuest(Guest newGuest)
        {
            if (await GuestExists(newGuest.Email))
            {
                throw new ConflictException($"Guest {newGuest.Email} already exists");
            }

            return await _db.QuerySingleAsync<Guest>(
                "INSERT INTO Guests(Email, Name, Surname) Values(@Email, @Name, @Surname) RETURNING *",
                newGuest
            );
        }

        public async Task<Guest> UpdateGuest(string email, Guest updatedGuest)
        {
            var existing = await GetGuestByEmail(email);

            var updated = await _db.QuerySingleAsync<Guest>(
                "UPDATE Guests SET Name = @Name, Surname = @Surname WHERE Email = @Email RETURNING *",
                new { existing.Email, updatedGuest.Name, updatedGuest.Surname }
            );

            return updated;
        }

        public async Task<Guest> GetOrCreateGuest(string email)
        {
            var existing = await _db.QueryFirstOrDefaultAsync<Guest>(
                "SELECT * FROM Guests WHERE Email = @email;",
                new { email }
            );

            if (existing != null)
            {
                return existing;
            }

            // Derive a placeholder name from the local part of the email so the
            // NOT NULL constraint on Name is satisfied without requiring the UI
            // to collect it separately during booking.
            var name = email.Split('@')[0];

            return await _db.QuerySingleAsync<Guest>(
                "INSERT INTO Guests(Email, Name, Surname) VALUES(@Email, @Name, NULL) RETURNING *",
                new { Email = email, Name = name }
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
