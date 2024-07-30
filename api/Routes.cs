/*
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
        public static void AddReservationRoutes(this WebApplication app)
        {
            app.MapGet(
                "/reservation",
                (SqliteConnection db) =>
                    db.GetAllOrEmpty<DbReservation>("SELECT * FROM Reservations;")
            );

            app.MapGet(
                "/reservation/{reservationId}",
                async (Guid reservationId, SqliteConnection db) =>
                {
                    var result = await db.QuerySingleOrDefaultAsync<DbReservation>(
                        "SELECT * FROM Reservations WHERE Id = @reservationId;",
                        new { reservationId = reservationId.ToString() }
                    );

                    if (result == null)
                    {
                        return Results.NotFound();
                    }

                    return Results.Ok(
                        new Reservation
                        {
                            Id = Guid.Parse(result.Id),
                            GuestEmail = result.GuestEmail,
                            RoomNumber = result.RoomNumber,
                            Start = result.Start,
                            End = result.End
                        }
                    );
                }
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
                "/reservation/{reservationId}",
                async (Guid reservationId, SqliteConnection db) =>
                {
                    try
                    {
                        var result = await db.QuerySingleOrDefaultAsync(
                            "DELETE FROM Reservations WHERE ID = @reservationId;",
                            new { reservationId }
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
    }
}
*/
