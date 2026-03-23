using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Contracts;
using Models;
using Repositories;

namespace Controllers
{
    [ApiController]
    [Tags("Rooms"), Route("rooms")]
    public class RoomsController : ControllerBase
    {
        private RoomRepository _repo { get; set; }

        public RoomsController(RoomRepository roomRepository)
        {
            _repo = roomRepository;
        }

        [HttpGet, Produces("application/json"), Route("")]
        [ProducesResponseType(typeof(IEnumerable<Room>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Room>>> GetRooms()
        {
            var rooms = await _repo.GetRooms();

            if (rooms == null)
            {
                return Ok(Enumerable.Empty<Room>());
            }

            return Ok(rooms);
        }

        [HttpGet, Produces("application/json"), Route("{roomNumber}")]
        [ProducesResponseType(typeof(Room), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Room>> GetRoom(string roomNumber)
        {
            var room = await _repo.GetRoom(roomNumber);

            return Ok(room);
        }

        [Authorize]
        [HttpPost, Produces("application/json"), Route("")]
        [ProducesResponseType(typeof(Room), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Room>> CreateRoom([FromBody] Room newRoom)
        {
            var createdRoom = await _repo.CreateRoom(newRoom);

            if (createdRoom == null)
            {
                return NotFound();
            }

            return Ok(createdRoom);
        }

        [Authorize]
        [HttpPost, Produces("application/json"), Route("import")]
        [ProducesResponseType(typeof(ImportResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ImportResult>> ImportRooms(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { detail = "No file provided" });

            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { detail = "Only CSV files are accepted" });

            if (file.Length > 50_000) // ~500 rooms max
                return BadRequest(new { detail = "File is too large. Maximum 500 rooms supported" });

            var errors = new List<ImportError>();
            var validRooms = new List<(int Line, Room Room)>();
            var lineNumber = 0;

            // Parse and validate
            using var reader = new StreamReader(file.OpenReadStream());
            while (await reader.ReadLineAsync() is { } line)
            {
                lineNumber++;
                var roomNumber = line.Trim();

                if (string.IsNullOrEmpty(roomNumber))
                    continue;

                if (lineNumber == 1 && roomNumber.Equals("number", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!Room.IsValidRoomNumber(roomNumber))
                {
                    errors.Add(new ImportError(lineNumber, roomNumber, $"Invalid room number: {roomNumber}"));
                    continue;
                }

                validRooms.Add((lineNumber, new Room { Number = roomNumber }));
            }

            var imported = await _repo.CreateRoomsBatch(validRooms, errors);

            return Ok(new ImportResult(lineNumber, imported, errors.Count, errors));
        }

        [Authorize]
        [HttpPatch, Produces("application/json"), Route("{roomNumber}")]
        [ProducesResponseType(typeof(Room), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Room>> UpdateRoomState(string roomNumber, [FromBody] UpdateRoomStateRequest request)
        {
            var room = await _repo.UpdateRoomState(roomNumber, request.State);
            return Ok(room);
        }

        [Authorize]
        [HttpDelete, Produces("application/json"), Route("{roomNumber}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteRoom(string roomNumber)
        {
            if (roomNumber.Length != 3)
            {
                return BadRequest("Invalid room ID - format is ###, ex 001 / 002 / 101");
            }

            var deleted = await _repo.DeleteRoom(roomNumber);

            return deleted ? NoContent() : NotFound();
        }
    }
}
