using System.Data;
using Dapper;
using Db;
using Helpers;
using Microsoft.Data.Sqlite;
using Repositories;
using Repositories.Interfaces;
using Services;
using Validators;
using Validators.Interfaces;

var builder = WebApplication.CreateBuilder(args);

{
    var Services = builder.Services;
    var connectionString =
        builder.Configuration.GetConnectionString("ReservationsDb")
        ?? "Data Source=reservations.db;Cache=Shared";

    SqlMapper.AddTypeHandler(new GuidTypeHandler());
    SqlMapper.AddTypeHandler(new SqliteBooleanHandler());

    Services.AddSingleton(_ => new SqliteConnection(connectionString));
    Services.AddSingleton<IDbConnection>(sp => sp.GetRequiredService<SqliteConnection>());
    Services.AddScoped<IReservationRepository, ReservationRepository>();
    Services.AddScoped<IRoomRepository, RoomRepository>();
    Services.AddScoped<IGuestRepository, GuestRepository>();
    builder.Services.AddScoped<ICheckInService, CheckInService>();
    Services.AddSingleton<IReservationValidator, ReservationValidator>();
    Services
        .AddMvc(opt =>
        {
            opt.EnableEndpointRouting = false;
        })
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = null;
        });
    Services.AddCors();
    Services.AddEndpointsApiExplorer();
    Services.AddSwaggerGen();

    Services
        .AddAuthentication("StaffAuth")
        .AddCookie(
            "StaffAuth",
            options =>
            {
                options.Cookie.Name = "StaffAccess";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.LoginPath = "/staff/login";
                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                };
            }
        );

    Services.AddAuthorization();
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
