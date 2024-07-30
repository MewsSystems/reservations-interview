using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace Controllers
{
    [Tags("Guests"), Route("guest")]
    public class GuestController : Controller
    {
        private SqliteConnection db { get; set; }

        public GuestController(SqliteConnection sqliteDb)
        {
            db = sqliteDb;
        }

        [HttpGet, Produces("application/json"), Route("")]
        public async Task<ActionResult<Guest>> GetGuests()
        {
            var guests = await db.QueryAsync<Room>("SELECT * FROM Guests;");

            if (guests == null)
            {
                return Json(Enumerable.Empty<Guest>());
            }

            return Json(guests);
        }
    }
}
