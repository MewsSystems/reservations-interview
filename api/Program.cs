using Db;
using Microsoft.Data.Sqlite;
using Routes;

var builder = WebApplication.CreateBuilder(args);


{
    var connectionString =
        builder.Configuration.GetConnectionString("ReservationsDb")
        ?? "Data Source=reservations.db;Cache=Shared";
    builder.Services.AddCors();
    builder.Services.AddScoped(_ => new SqliteConnection(connectionString));
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
}

var app = builder.Build();


{
    try
    {
        Setup.EnsureDb(app.Services.CreateScope());
    }
    catch (Exception ex)
    {
        Console.WriteLine("Failed to setup the database, aboirting");
        Console.WriteLine(ex.ToString());
        Environment.Exit(1);
        return;
    }

    app.UsePathBase("/api").UseCors(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
    app.UseSwagger();
    app.UseSwaggerUI();

    app.AddRoomRoutes();
    app.AddGuestRoutes();
    app.AddReservationEndpoints();
}

app.Run();
