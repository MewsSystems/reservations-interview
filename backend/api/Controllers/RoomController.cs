using api.Shared.Models.Domain;
using api.Shared.Models.Errors;
using api.Shared.Services;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Tags("Rooms"), Route("room")]
    public class RoomController : Controller
    {
        private IRoomService _service { get; set; }

        public RoomController(IRoomService service)
        {
            _service = service;
        }

        [HttpGet, Produces("application/json"), Route("")]
        public async Task<ActionResult<Room>> GetRooms()
        {
            var rooms = await _service.Get();
            if (rooms == null)
            {
                return Json(Enumerable.Empty<Room>());
            }

            return Json(rooms);
        }

        [HttpGet, Produces("application/json"), Route("{roomNumber}")]
        public async Task<ActionResult<Room>> GetRoom(string roomNumber)
        {
            if (string.IsNullOrEmpty(roomNumber) || roomNumber.Length != 3)
            {
                return BadRequest("Invalid room ID - format is ###, ex 001 / 002 / 101");
            }

            try
            {
                var room = await _service.GetByRoomNumber(roomNumber);
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
            if (newRoom == null)
            {
                return BadRequest();
            }
            var createdRoom = await _service.Create(newRoom);

            if (createdRoom == null)
            {
                return NotFound();
            }

            return Json(createdRoom);
        }

        [HttpDelete, Produces("application/json"), Route("{roomNumber}")]
        public async Task<IActionResult> DeleteRoom(string roomNumber)
        {
            if (string.IsNullOrEmpty(roomNumber) || roomNumber.Length != 3)
            {
                return BadRequest("Invalid room ID - format is ###, ex 001 / 002 / 101");
            }

            var deleted = await _service.DeleteByRoomNumber(roomNumber);

            return deleted ? NoContent() : NotFound();
        }
    }
}
