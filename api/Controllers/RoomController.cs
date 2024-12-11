using Microsoft.AspNetCore.Mvc;
using Models;
using Models.Errors;
using Models.Validators;
using Repositories;
using Microsoft.Extensions.Caching.Memory;

namespace Controllers
{
    [Tags("Rooms"), Route("room")]
    public class RoomController : Controller
    {
        private RoomRepository _repo { get; set; }
        private readonly RoomValidator _roomValidator;
        private readonly ILogger<RoomController> _logger;
        private readonly IMemoryCache _cache;

        public RoomController(RoomRepository roomRepository, RoomValidator roomValidator, ILogger<RoomController> logger, IMemoryCache memoryCache)
        {
            _repo = roomRepository;
            _roomValidator = roomValidator;
            _logger = logger;
            _cache = memoryCache;
        }

        [HttpGet, Produces("application/json"), Route("")]
        public async Task<ActionResult<Room>> GetRooms()
        {
            try
            {
                var rooms = await _repo.GetRooms();

                if (rooms == null)
                {
                    _logger.LogWarning("No rooms found in the database.");
                    return Json(Enumerable.Empty<Room>());
                }

                _logger.LogInformation("Retrieved all rooms from the database.");
                return Json(rooms);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching rooms.");
                return StatusCode(500, "An error occurred while fetching rooms.");
            }


        }

        [HttpGet, Produces("application/json"), Route("{roomNumber}")]
        public async Task<ActionResult<Room>> GetRoom(string roomNumber)
        {
            if (!_roomValidator.IsValidRoomNumber(roomNumber))
            {
                _logger.LogWarning("Invalid room number format: {RoomNumber}", roomNumber);
                return BadRequest("Invalid room number. Ensure it follows the proper format and rules.");
            }

            try
            {
                var room = await _repo.GetRoom(roomNumber);

                if (room == null)
                {
                    _logger.LogWarning("Room not found: {RoomNumber}", roomNumber);
                    return NotFound();
                }

                _logger.LogInformation("Room retrieved successfully: {RoomNumber}", roomNumber);
                return Json(room);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching the room with number {RoomNumber}.", roomNumber);
                return StatusCode(500, "An error occurred while fetching the room.");
            }
        }

        [HttpPost, Produces("application/json"), Route("")]
        public async Task<ActionResult<Room>> CreateRoom([FromBody] Room newRoom)
        {
            if (!_roomValidator.IsValidRoomNumber(newRoom.Number))


            {
                _logger.LogWarning("Invalid room number format: {RoomNumber}", newRoom.Number);
                return BadRequest("Invalid room number. Ensure it follows the proper format and rules.");
            }

            try
            {
                var createdRoom = await _repo.CreateRoom(newRoom);

                if (createdRoom == null)
                {
                    _logger.LogWarning("Failed to create room with number: {RoomNumber}", newRoom.Number);
                    return NotFound();
                }

                _logger.LogInformation("Room created successfully: {RoomNumber}", newRoom.Number);
                return Json(createdRoom);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the room with number {RoomNumber}.", newRoom.Number);
                return StatusCode(500, "An error occurred while creating the room.");
            }
        }

        [HttpDelete, Produces("application/json"), Route("{roomNumber}")]
        public async Task<IActionResult> DeleteRoom(string roomNumber)
        {
            if (roomNumber.Length != 3)
            {
                return BadRequest("Invalid room ID - format is ###, ex 001 / 002 / 101");
            }

            var deleted = await _repo.DeleteRoom(roomNumber);

            return deleted ? NoContent() : NotFound();
        }

        [HttpGet, Produces("application/json"), Route("checkRoomAvailability")]
        public async Task<ActionResult> CheckRoomAvailability(string roomNumber, string startDate, string endDate)
        {
            try
            {
                string cacheKey = $"{roomNumber}_{startDate}_{endDate}";

                if (!_cache.TryGetValue(cacheKey, out bool isAvailable))
                {
                    isAvailable = await _repo.CheckRoomAvailability(roomNumber, startDate, endDate);

                    _cache.Set(cacheKey, isAvailable, TimeSpan.FromMinutes(15));
                }

                return Json(new { available = isAvailable });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while checking room availability.");
                return StatusCode(500, "An error occurred while checking availability.");
            }
        }
    }
}
