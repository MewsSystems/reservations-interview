using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.Errors;
using Repositories;

namespace Controllers
{
    [Tags("Rooms"), Route("room"), Authorize(Policy = "StaffOnly")]
    public class RoomController : Controller
    {
        private RoomRepository _repo { get; set; }

        public RoomController(RoomRepository roomRepository)
        {
            _repo = roomRepository;
        }

        [HttpGet, Produces("application/json"), Route("")]
        public async Task<ActionResult<Room>> GetRooms()
        {
            var rooms = await _repo.GetRooms();

            if (rooms == null)
            {
                return Json(Enumerable.Empty<Room>());
            }

            return Json(rooms);
        }

        [HttpGet, Produces("application/json"), Route("{roomNumber}")]
        public async Task<ActionResult<Room>> GetRoom(string roomNumber)
        {
            if (!Room.IsValidRoomNumber(roomNumber))
            {
                return BadRequest("Invalid room ID - must be exactly 3 digits and the last two digits cannot be 00 (e.g. 101, 202).");
            }

            try
            {
                var room = await _repo.GetRoom(roomNumber);

                return Json(room);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost, Produces("application/json"), Route("")]
        public async Task<ActionResult<Room>> CreateRoom([FromBody] Room newRoom)
        {
            if (!Room.IsValidRoomNumber(newRoom.Number))
            {
                return BadRequest("Invalid room ID - must be exactly 3 digits and the last two digits cannot be 00 (e.g. 101, 202).");
            }

            var createdRoom = await _repo.CreateRoom(newRoom);

            if (createdRoom == null)
            {
                return NotFound();
            }

            return Json(createdRoom);
        }

        [HttpDelete, Produces("application/json"), Route("{roomNumber}")]
        public async Task<IActionResult> DeleteRoom(string roomNumber)
        {
            if (!Room.IsValidRoomNumber(roomNumber))
            {
                return BadRequest("Invalid room ID - must be exactly 3 digits and the last two digits cannot be 00 (e.g. 101, 202).");
            }

            var deleted = await _repo.DeleteRoom(roomNumber);

            return deleted ? NoContent() : NotFound();
        }

        [HttpPut, Produces("application/json"), Route("{roomNumber}/state")]
        public async Task<IActionResult> SetRoomState(string roomNumber, [FromBody] SetRoomStateRequest? request)
        {
            if (!Room.IsValidRoomNumber(roomNumber))
            {
                return BadRequest("Invalid room ID - must be exactly 3 digits and the last two digits cannot be 00 (e.g. 101, 202).");
            }

            if (request is null)
            {
                return BadRequest("Invalid payload.");
            }

            try
            {
                await _repo.SetRoomState(roomNumber, request.State);
                return NoContent();
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }
    }

    public record SetRoomStateRequest(Models.State State);
}
