using api.Shared.Db;
using api.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);
{
    var services = builder.Services;
    var connectionString =
        builder.Configuration.GetConnectionString("ReservationsDb")
        ?? "Data Source=reservations.db;Cache=Shared";
    
    // Shared services
    services.AddReservationServices(connectionString);

    services.AddMvc(opt =>
    {
        opt.EnableEndpointRouting = false;
    });
    services.AddCors();
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();
}

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

{
    try
    {
        using var scope = app.Services.CreateScope();
        Setup.EnsureDb(scope.ServiceProvider);
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
