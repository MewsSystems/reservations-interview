using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace Controllers
{
    [Tags("Reservations"), Route("reservation")]
    public class ReservationController : Controller
    {
        private SqliteConnection db { get; set; }

        public ReservationController(SqliteConnection sqliteDb)
        {
            db = sqliteDb;
        }

        [HttpGet, Produces("application/json"), Route("")]
        public async Task<ActionResult<Reservation>> GetReservations()
        {
            var reservations = await db.QueryAsync<DbReservation>("SELECT * FROM Reservations;");

            if (reservations == null)
            {
                return Json(Enumerable.Empty<Reservation>());
            }

            return Json(reservations.Select(r => r.ToDomain()));
        }
    }
}
