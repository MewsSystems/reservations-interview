using Db;
using Microsoft.Data.Sqlite;

var builder = WebApplication.CreateBuilder(args);


{
    var Services = builder.Services;
    var connectionString =
        builder.Configuration.GetConnectionString("ReservationsDb")
        ?? "Data Source=reservations.db;Cache=Shared";

    Services.AddMvc(opt =>
    {
        opt.EnableEndpointRouting = false;
    });
    Services.AddCors();
    Services.AddScoped(_ => new SqliteConnection(connectionString));
    Services.AddEndpointsApiExplorer();
    Services.AddSwaggerGen();
}

var app = builder.Build();


{
    try
    {
        Setup.EnsureDb(app.Services.CreateScope());
    }
    catch (Exception ex)
    {
        Console.WriteLine("Failed to setup the database, aborting");
        Console.WriteLine(ex.ToString());
        Environment.Exit(1);
        return;
    }

    app.UsePathBase("/api")
        .UseMvc()
        .UseCors(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader())
        .UseSwagger()
        .UseSwaggerUI();
}

app.Run();
