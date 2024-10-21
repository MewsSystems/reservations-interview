using Db;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Models;
using Repositories;

namespace Api.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            if (File.Exists("reservations.db"))
            {
                File.Delete("reservations.db");
            }
            var sp = services.BuildServiceProvider();

            using var scope = sp.CreateScope();
            Setup.EnsureDb(scope);

            var scopedServices = scope.ServiceProvider;
            var roomRepository = scopedServices.GetRequiredService<RoomRepository>();
            var guestRepository = scopedServices.GetRequiredService<GuestRepository>();

            // Seed test guest and rooms
            try
            {
                guestRepository.CreateGuest(new Guest { Email = "test@test.it", Name = "test" }).GetAwaiter().GetResult();
                guestRepository.CreateGuest(new Guest { Email = "test2@test.it", Name = "test2" }).GetAwaiter().GetResult();
                for (var i = 0; i < 5; i++)
                {
                    roomRepository.CreateRoom(new Room { Number = $"00{i}", State = State.Ready }).GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred seeding the database with test messages. Error: {ex.Message}");
            }
        });
    }
}
