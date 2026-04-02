using System.Data;
using Db;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Data.Sqlite;
using Models;
using Repositories;
using Serilog;
using Services;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, config) => config
    .ReadFrom.Configuration(ctx.Configuration)
    .WriteTo.Console());


{
    var Services = builder.Services;
    var connectionString =
        builder.Configuration.GetConnectionString("ReservationsDb")
        ?? "Data Source=reservations.db;Cache=Shared";

    Services.AddScoped(_ => new SqliteConnection(connectionString));
    Services.AddScoped<IDbConnection>(sp => sp.GetRequiredService<SqliteConnection>());
    Services.AddScoped<GuestRepository>();
    Services.AddScoped<RoomRepository>();
    Services.AddScoped<ReservationRepository>();
    Services.AddSingleton<VerificationCodeService>();
    Services.Configure<ImportOptions>(builder.Configuration.GetSection("Import"));
    Services.AddMvc(opt =>
    {
        opt.EnableEndpointRouting = false;
    }).AddNewtonsoftJson();
    Services.AddCors();
    Services
        .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
                ? CookieSecurePolicy.None
                : CookieSecurePolicy.Always;
            options.SlidingExpiration = true;
            options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
            options.Events.OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = 401;
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = 403;
                return Task.CompletedTask;
            };
        });
    Services.AddAuthorization();
    Services.AddEndpointsApiExplorer();
    Services.AddSwaggerGen();
}

var app = builder.Build();


{
    try
    {
        using var scope = app.Services.CreateScope();
        await Setup.EnsureDb(scope);
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "Failed to setup the database, aborting");
        Environment.Exit(1);
        return;
    }

    app.UsePathBase("/api");
    app.UseSerilogRequestLogging();

    app.UseCors(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()
        .WithExposedHeaders("X-Total-Count", "X-Page", "X-Page-Size"));

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler(err =>
            err.Run(async context =>
            {
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(
                    new { errors = new[] { "An unexpected error occurred." } }
                );
            })
        );
    }
    
    app.UseAuthentication();
    app.UseAuthorization();

    app
        .UseMvc()
        .UseSwagger()
        .UseSwaggerUI();
}

app.Run();
