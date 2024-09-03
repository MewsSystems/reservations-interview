using Microsoft.Extensions.DependencyInjection;
using api.Shared.Extensions;
using System.Data;
using api.Shared.Db;
using api.Shared.Repositories.Dapper;
using api.Shared.Repositories;
using api.Shared.Models.DB;

namespace api.Tests.Integration
{
    public class DatabaseSeedFixture : DatabaseFixture
    {
        public string[] Guests = { "guest1@example.com", "guest2@example.com" };
        public int[] Rooms = { 111, 112 };

        public DatabaseSeedFixture() : base()
        {
            using var dbConnection = _serviceProvider.GetRequiredService<IDbConnection>();
            SeedDatabase(dbConnection);
        }

        protected void SeedDatabase(IDbConnection dbConnection)
        {
            var guestRepository = new GuestRepository(dbConnection);
            var roomRepository = new RoomRepository(dbConnection);
            foreach (var guest in Guests)
            {
                guestRepository.CreateGuest(new Guest { Email = guest, Name = guest, Surname = guest }).Wait();
            }
            foreach (var roomNumber in Rooms)
            {
                roomRepository.CreateRoom(new Room { Number = roomNumber }).Wait();
            }
        }
    }
}
