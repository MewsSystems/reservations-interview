using System.Net;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Models;

namespace Api.IntegrationTests;

public class StaffControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly WebApplicationFactory<Program> _factory;

    private readonly JsonSerializerOptions _options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public StaffControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }
    
    [Fact]
    public async Task GetReservations_Should_Return_Todays_And_Future_Reservations()
    {
        // Arrange
        
        // creates an authenticated client
        var client = _factory.CreateDefaultClient(new CookieDelegatingHandler("access", "1"));
        await CreateReservation(client, new Reservation
        {
            // this is in the past
            RoomNumber = "001",
            GuestEmail = "test@test.it",
            Start = DateTime.Now.AddDays(-10),
            End = DateTime.Now.AddDays(-8),
        });
        await CreateReservation(client, new Reservation
        {
            // today
            RoomNumber = "001",
            GuestEmail = "test@test.it",
            Start = DateTime.Now,
            End = DateTime.Now.AddDays(2),
        });
        await CreateReservation(client, new Reservation
        {
            // future
            RoomNumber = "001",
            GuestEmail = "test@test.it",
            Start = DateTime.Now.AddDays(10),
            End = DateTime.Now.AddDays(12),
        });

        // Act
        var response = await client.GetAsync("api/staff/reservations");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var reservations = JsonSerializer.Deserialize<Reservation[]>(content, _options);
        Assert.NotNull(reservations);
        Assert.Equal(2, reservations.Length);
        Assert.All(reservations, r => Assert.True(r.Start >= DateTime.Today));
    }

    private async Task CreateReservation(HttpClient client, Reservation reservation)
    {
        var httpContent = new StringContent(JsonSerializer.Serialize(reservation), Encoding.UTF8, MediaTypeNames.Application.Json);
        var response = await client.PostAsync("api/reservation", httpContent);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.True(response.IsSuccessStatusCode, content);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}

public class CookieDelegatingHandler : DelegatingHandler
{
    private readonly string _cookieName;
    private readonly string _cookieValue;

    public CookieDelegatingHandler(string cookieName, string cookieValue)
    {
        _cookieName = cookieName;
        _cookieValue = cookieValue;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Add the cookie to the request header
        request.Headers.Add("Cookie", $"{_cookieName}={_cookieValue}");

        // Call the inner handler to continue the request processing
        return await base.SendAsync(request, cancellationToken);
    }
}
