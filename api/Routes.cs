using System.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

class ValidationError
{
    public required string Message { get; set; }
}

namespace Routes
{
    public static class RouteExtensions
    {
        /// <summary>
        /// Extension helper to return an empty collection
        /// or the list of results
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<T>> GetAllOrEmpty<T>(
            this IDbConnection conn,
            string sql
        )
        {
            try
            {
                var rows = await conn.QueryAsync<T>(sql);
                if (rows != null)
                {
                    return rows;
                }
            }
            catch { }

            return [];
        }

        /// <summary>
        /// Ensures this is a staff member, if not returns true and 403
        /// </summary>
        /// <param name="request"></param>
        public static bool IsNotStaff(HttpRequest request, out IResult? result)
        {
            // TODO explore UseAuthentication
            request.Cookies.TryGetValue("access", out string? accessValue);

            if (accessValue == null || accessValue == "0")
            {
                result = Results.StatusCode(403);
                return true;
            }

            result = null;
            return false;
        }

        public static void AddRoomRoutes(this WebApplication app)
        {
            app.MapGet(
                "/room",
                (SqliteConnection db) => db.GetAllOrEmpty<Room>("SELECT * FROM Rooms;")
            );

            app.MapGet(
                "/room/{roomNumber}",
                async (string roomNumber, SqliteConnection db) =>
                {
                    if (roomNumber.Length != 3)
                    {
                        return Results.BadRequest(
                            new ValidationError { Message = "Invalid Room Number" }
                        );
                    }
                    var roomNumberInt = -1;
                    int.TryParse(roomNumber, out roomNumberInt);

                    if (roomNumberInt < 0 || roomNumberInt > 999)
                    {
                        return Results.BadRequest(
                            new ValidationError { Message = "Invalid Room Number" }
                        );
                    }

                    var result = await db.QuerySingleOrDefaultAsync<Room>(
                        "SELECT * FROM Rooms WHERE Number = @roomNumberInt",
                        new { roomNumberInt }
                    );
                    if (result is Room room)
                    {
                        return Results.Ok(room);
                    }

                    return Results.NotFound();
                }
            );

            app.MapPost(
                "/room",
                async (Room room, SqliteConnection db) =>
                {
                    if (room.Number < 0 || room.Number > 999)
                    {
                        var error = new ValidationError { Message = "Invalid Room Number" };
                        return Results.BadRequest(error);
                    }

                    var newRoom = await db.QuerySingleAsync<Room>(
                        "INSERT INTO Rooms(Number, State) Values(@Number, @State) RETURNING *",
                        room
                    );

                    return Results.Created($"/room/{newRoom.FormatRoomNumber()}", newRoom);
                }
            );

            app.MapDelete(
                "/room",
                async (int roomNumber, SqliteConnection db) =>
                {
                    try
                    {
                        var result = await db.QuerySingleOrDefaultAsync(
                            "DELETE FROM Rooms WHERE Number = @roomNumber;",
                            new { roomNumber }
                        );

                        return result == 1 ? Results.NotFound() : Results.NoContent();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("An error occured while deleting a room");
                        Console.WriteLine(ex.ToString());
                        return Results.BadRequest();
                    }
                }
            );
        }

        public static void AddGuestRoutes(this WebApplication app)
        {
            app.MapGet(
                "/guest",
                (SqliteConnection db) => db.GetAllOrEmpty<Guest>("SELECT * FROM Guests;")
            );
        }

        public static void AddReservationRoutes(this WebApplication app)
        {
            app.MapGet(
                "/reservation",
                (SqliteConnection db) =>
                    db.GetAllOrEmpty<DbReservation>("SELECT * FROM Reservations;")
            );

            app.MapPost(
                "/reservation",
                async (NewReservation newBooking, SqliteConnection db) =>
                {
                    var newReservation = new Reservation
                    {
                        Id = Guid.NewGuid(),
                        GuestEmail = newBooking.GuestEmail,
                        RoomNumber = newBooking.RoomNumber,
                        Start = newBooking.Start,
                        End = newBooking.End
                    };

                    try
                    {
                        // ?? EnsureGuest
                        // TODO impl persistence

                        await Task.CompletedTask;
                        return Results.Created(
                            $"/reservation/${newReservation.Id}",
                            newReservation
                        );
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("An error occured when trying to book a reservation:");
                        Console.WriteLine(ex.ToString());

                        return Results.BadRequest("Invalid reservation");
                    }
                }
            );

            app.MapDelete(
                "/reservation",
                async (Guid reservationId, SqliteConnection db) =>
                {
                    try
                    {
                        var result = await db.QuerySingleAsync(
                            "DELETE FROM Reservations WHERE ID = @reservationId",
                            new { reservationId }
                        );

                        return result == 1 ? Results.NoContent() : Results.NotFound();
                    }
                    catch
                    {
                        return Results.NotFound();
                    }
                }
            );
        }

        public static void AddStaffRoutes(this WebApplication app)
        {
            app.MapGet(
                "/staff/login",
                (
                    IConfiguration config,
                    [FromHeader(Name = "X-Staff-Code")] string accessCode,
                    HttpResponse response
                ) =>
                {
                    var configuredSecret = config.GetValue<string>("staffAccessCode");
                    if (configuredSecret != accessCode)
                    {
                        // don't set cookie, don't indicate anything
                        return Results.NoContent();
                    }
                    response.Cookies.Append(
                        "access",
                        "1",
                        new CookieOptions
                        // TODO evaluate cookie options & auth mechanism for best security practices
                        {
                            IsEssential = true,
                            SameSite = SameSiteMode.Strict,
                            HttpOnly = true,
                            Secure = false
                        }
                    );
                    return Results.NoContent();
                }
            );

            app.MapGet(
                "/staff/check",
                (HttpRequest request) =>
                {
                    if (IsNotStaff(request, out IResult? result))
                    {
                        return result;
                    }

                    return Results.Ok("Authorized");
                }
            );

            // TODO set up staff routes
        }
    }
}
