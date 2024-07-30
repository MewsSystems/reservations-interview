using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace Controllers
{
    [Tags("Rooms"), Route("room")]
    public class RoomController : Controller
    {
        private SqliteConnection db { get; set; }

        public RoomController(SqliteConnection sqliteDb)
        {
            db = sqliteDb;
        }

        [HttpGet, Produces("application/json"), Route("")]
        public async Task<ActionResult<Room>> GetRooms()
        {
            var rooms = await db.QueryAsync<Room>("SELECT * FROM Rooms;");

            if (rooms == null)
            {
                return Json(Enumerable.Empty<Room>());
            }

            return Json(rooms);
        }

        [HttpGet, Produces("application/json"), Route("{roomNumber}")]
        public async Task<ActionResult<Room>> GetRoom(string roomNumber)
        {
            if (roomNumber.Length != 3)
            {
                return BadRequest("Invalid room ID - format is ###, ex 001 / 002 / 101");
            }

            var success = int.TryParse(roomNumber, out int roomNumberInt);
            if (!success)
            {
                return BadRequest("Invalid room ID - format is ###, ex 001 / 002 / 101");
            }

            var room = await db.QueryFirstOrDefaultAsync<Room>(
                "SELECT * FROM Rooms WHERE Number = @roomNumberInt;",
                new { roomNumberInt }
            );

            if (room == null)
            {
                return NotFound();
            }

            return Json(room);
        }

        [HttpPost, Produces("application/json"), Route("")]
        public async Task<ActionResult<Room>> CreateRoom(Room newRoom)
        {
            if (newRoom.Number < 1 || newRoom.Number > 999)
            {
                return BadRequest(
                    "Invalid room ID - format is ###, ex 001 / 002 / 101, from 1 - 999"
                );
            }

            var createdRoom = await db.QuerySingleAsync<Room>(
                "INSERT INTO Rooms(Number, State) Values(@Number, @State) RETURNING *",
                newRoom
            );

            if (createdRoom == null)
            {
                return NotFound();
            }

            return Json(createdRoom);
        }

        [HttpDelete, Produces("application/json"), Route("{roomNumber}")]
        public async Task<IActionResult> DeleteRoom(string roomNumber)
        {
            if (roomNumber.Length != 3)
            {
                return BadRequest("Invalid room ID - format is ###, ex 001 / 002 / 101");
            }

            var success = int.TryParse(roomNumber, out int roomNumberInt);
            if (!success)
            {
                return BadRequest("Invalid room ID - format is ###, ex 001 / 002 / 101");
            }

            var result = await db.QuerySingleOrDefaultAsync(
                "DELETE FROM Rooms WHERE Number = @roomNumberInt;",
                new { roomNumberInt }
            );

            return result == 1 ? NotFound() : NoContent();
        }
    }
}
