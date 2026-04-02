using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Models;
using Models.Errors;
using Repositories;
using Extensions;

namespace Controllers
{
    [Tags("Rooms"), Route("room")]
    public class RoomController : Controller
    {
        private static readonly ActivitySource _activitySource = new("Reservations.RoomImport");

        private RoomRepository _repo { get; set; }
        private ImportOptions _importOptions { get; set; }
        private ILogger<RoomController> _log { get; set; }

        public RoomController(RoomRepository roomRepository, IOptions<ImportOptions> importOptions, ILogger<RoomController> log)
        {
            _repo = roomRepository;
            _importOptions = importOptions.Value;
            _log = log;
        }

        [HttpGet, Produces("application/json"), Route("")]
        [AllowAnonymous]
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
        [AllowAnonymous]
        public async Task<ActionResult<Room>> GetRoom(string roomNumber)
        {
            if (roomNumber.Length != 3)
            {
                _log.LogWarning("GetRoom invalid format: {RoomNumber}", roomNumber);
                return BadRequest(new { errors = new[] { "Invalid room ID - format is ###, ex 001 / 002 / 101" } });
            }

            try
            {
                var room = await _repo.GetRoom(roomNumber);
                return Json(room);
            }
            catch (NotFoundException)
            {
                _log.LogWarning("GetRoom not found: {RoomNumber}", roomNumber);
                return NotFound();
            }
        }

        [HttpPost, Produces("application/json"), Route("")]
        [Authorize]
        public async Task<ActionResult<Room>> CreateRoom([FromBody] Room newRoom)
        {
            var errors = newRoom.Validate();
            if (errors.Count > 0)
            {
                _log.LogWarning("CreateRoom validation failed for {RoomNumber}: {ErrorCount} errors", newRoom.Number, errors.Count);
                return BadRequest(new { errors });
            }

            var createdRoom = await _repo.CreateRoom(newRoom);

            if (createdRoom == null)
            {
                _log.LogWarning("CreateRoom failed — room {RoomNumber} not created", newRoom.Number);
                return NotFound();
            }

            _log.LogInformation("Created room {RoomNumber} with State={State}, IsDirty={IsDirty}",
                createdRoom.Number, createdRoom.State, createdRoom.IsDirty);
            return Json(createdRoom);
        }

        private static readonly HashSet<string> AllowedPatchPaths =
            new(StringComparer.OrdinalIgnoreCase) { $"/{nameof(RoomPatch.IsDirty)}" };

        [HttpPatch, Produces("application/json"), Route("{roomNumber}")]
        [Authorize]
        public async Task<IActionResult> PatchRoom(
            string roomNumber, [FromBody] JsonPatchDocument<RoomPatch> patchDoc)
        {
            if (roomNumber.Length != 3)
            {
                _log.LogWarning("PatchRoom invalid format: {RoomNumber}", roomNumber);
                return BadRequest(new { errors = new[] { "Invalid room ID - format is ###, ex 001 / 002 / 101" } });
            }

            // Reject operations on paths we don't support
            var disallowed = patchDoc.Operations
                .Where(op => !AllowedPatchPaths.Contains(op.path))
                .Select(op => op.path)
                .Distinct()
                .ToList();

            if (disallowed.Count > 0)
            {
                return BadRequest(new { errors = disallowed.Select(p => $"Patching '{p}' is not allowed.").ToArray() });
            }

            Room room;
            try
            {
                room = await _repo.GetRoom(roomNumber);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }

            var patchModel = new RoomPatch();

            patchDoc.ApplyTo(patchModel, ModelState);
            if (!ModelState.IsValid)
            {
                return BadRequest(new { errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray() });
            }

            if (patchModel.IsDirty != null)
            {
                await _repo.SetRoomDirtyState(roomNumber, patchModel.IsDirty.Value);
                _log.LogInformation("Patched room {RoomNumber}: IsDirty={IsDirty}", roomNumber, patchModel.IsDirty.Value);
            }
            var updated = await _repo.GetRoom(roomNumber);
            return Ok(updated);
        }


        [HttpPost, Produces("application/json"), Consumes("multipart/form-data"), Route("import")]
        [Authorize]
        public async Task<IActionResult> ImportRooms(IFormFile file, CancellationToken ct)
        {
            var maxFileSize = _importOptions.MaxFileSizeBytes;
            var maxRows = _importOptions.MaxRows;

            if (file == null || file.Length == 0)
            {
                return BadRequest(new { errors = new[] { "A CSV file is required." } });
            }

            if (file.Length > maxFileSize)
            {
                return BadRequest(new { errors = new[] { $"File exceeds the maximum size of {maxFileSize / 1024} KB." } });
            }

            // Server-side file type check: extension
            var ext = Path.GetExtension(file.FileName);
            if (!string.Equals(ext, ".csv", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { errors = new[] { "Only .csv files are accepted." } });
            }

            // Stream-read lines with early bail at row limit
            var rows = new List<string>();
            var hasHeader = false;
            using var stream = file.OpenReadStream();
            using var parseSpan = _activitySource.StartActivity("ImportRooms.Parse");
            using (var reader = new StreamReader(stream))
            {
                while (await reader.ReadLineAsync(ct) is { } line)
                {
                    var trimmed = line.Trim();
                    if (string.IsNullOrEmpty(trimmed)) continue;

                    // Detect and skip header row (matches "Number" alone or "Number,State,IsDirty" pattern)
                    if (rows.Count == 0 && trimmed.Split(',')[0].Trim().Equals("Number", StringComparison.OrdinalIgnoreCase))
                    {
                        hasHeader = true;
                        continue;
                    }

                    rows.Add(trimmed);

                    if (rows.Count > maxRows)
                    {
                        return BadRequest(new { errors = new[] { $"CSV exceeds the maximum of {maxRows} data rows." } });
                    }
                }
            }
            parseSpan?.SetTag("import.parsed_rows", rows.Count);
            parseSpan?.Stop();

            if (rows.Count == 0)
            {
                return BadRequest(new { errors = new[] { "CSV file contains no data rows." } });
            }

            // Fetch existing rooms once for O(1) duplicate check
            var existingNumbers = await _repo.GetExistingRoomNumbers();

            var roomsToInsert = new List<Room>();
            var errors = new List<object>();
            var seenInBatch = new HashSet<string>();

            // Row numbers are 1-indexed relative to the original file
            var rowOffset = hasHeader ? 2 : 1;

            using var validateSpan = _activitySource.StartActivity("ImportRooms.Validate");
            for (int i = 0; i < rows.Count; i++)
            {
                var rowNumber = i + rowOffset;
                var columns = rows[i].Split(',');

                // Column 1: Number (required)
                var rawNumber = columns[0].Trim().TrimStart('0').PadLeft(3, '0');

                // Column 2: State (optional, defaults to Ready)
                var state = State.Ready;
                if (columns.Length > 1)
                {
                    var stateVal = columns[1].Trim();
                    if (!string.IsNullOrEmpty(stateVal) && !Enum.TryParse<State>(stateVal, ignoreCase: true, out state))
                    {
                        errors.Add(new { row = rowNumber, message = $"Invalid State '{stateVal}'. Must be Ready or Occupied." });
                        continue;
                    }
                }

                // Column 3: IsDirty (optional, defaults to false)
                var isDirty = false;
                if (columns.Length > 2)
                {
                    var dirtyVal = columns[2].Trim();
                    if (!string.IsNullOrEmpty(dirtyVal) && !bool.TryParse(dirtyVal, out isDirty))
                    {
                        errors.Add(new { row = rowNumber, message = $"Invalid IsDirty '{dirtyVal}'. Must be true or false." });
                        continue;
                    }
                }

                var room = new Room { Number = rawNumber, State = state, IsDirty = isDirty };
                var validationErrors = room.Validate();

                if (validationErrors.Count > 0)
                {
                    foreach (var err in validationErrors)
                    {
                        errors.Add(new { row = rowNumber, message = err });
                    }
                    continue;
                }

                // Duplicate in DB
                var roomInt = RoomExtensions.ConvertRoomNumberToInt(rawNumber);
                if (existingNumbers.Contains(roomInt))
                {
                    errors.Add(new { row = rowNumber, message = $"Room #{rawNumber} already exists." });
                    continue;
                }

                // Duplicate within same CSV
                if (!seenInBatch.Add(rawNumber))
                {
                    errors.Add(new { row = rowNumber, message = $"Room #{rawNumber} is a duplicate in this file." });
                    continue;
                }

                roomsToInsert.Add(room);
            }
            validateSpan?.SetTag("import.valid", roomsToInsert.Count);
            validateSpan?.SetTag("import.invalid", errors.Count);
            validateSpan?.Stop();

            var imported = 0;
            if (roomsToInsert.Count > 0)
            {
                using var insertSpan = _activitySource.StartActivity("ImportRooms.BulkInsert");
                imported = await _repo.BulkCreateRooms(roomsToInsert, ct);
                insertSpan?.SetTag("import.inserted", imported);
            }

            Activity.Current?.SetTag("import.imported", imported);
            Activity.Current?.SetTag("import.errors", errors.Count);
            Activity.Current?.SetTag("import.total_rows", rows.Count);
            Activity.Current?.SetTag("import.file_name", file.FileName);

            _log.LogInformation("Room import completed: {Imported} imported, {Errors} errors, {TotalRows} rows parsed from {FileName}",
                imported, errors.Count, rows.Count, file.FileName);

            return Ok(new { imported, errors });
        }

        [HttpDelete, Produces("application/json"), Route("{roomNumber}")]
        [Authorize]
        public async Task<IActionResult> DeleteRoom(string roomNumber)
        {
            if (roomNumber.Length != 3)
            {
                _log.LogWarning("DeleteRoom invalid format: {RoomNumber}", roomNumber);
                return BadRequest(new { errors = new[] { "Invalid room ID - format is ###, ex 001 / 002 / 101" } });
            }

            var deleted = await _repo.DeleteRoom(roomNumber);

            if (deleted)
                _log.LogInformation("Deleted room {RoomNumber}", roomNumber);
            else
                _log.LogWarning("DeleteRoom not found: {RoomNumber}", roomNumber);

            return deleted ? NoContent() : NotFound();
        }
    }
}
