using System.Net;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Models;

namespace Api.IntegrationTests;

public class ReservationControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly WebApplicationFactory<Program> _factory;

    private readonly JsonSerializerOptions _options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public ReservationControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task BookReservation_Should_Return_Created()
    {
        // Arrange
        var client = _factory.CreateClient();
        var expectedReservation = new Reservation
        {
            RoomNumber = "001",
            GuestEmail = "test@test.it",
            Start = DateTime.Now,
            End = DateTime.Now.AddDays(2),
        };

        // Act
        var httpContent = new StringContent(JsonSerializer.Serialize(expectedReservation), Encoding.UTF8, MediaTypeNames.Application.Json);
        var response = await client.PostAsync("api/reservation", httpContent);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.True(response.IsSuccessStatusCode, content);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var actualRreservation = JsonSerializer.Deserialize<Reservation>(content, _options);
        Assert.NotNull(actualRreservation);
        Assert.Equal(expectedReservation.GuestEmail, actualRreservation.GuestEmail);
        Assert.Equal(expectedReservation.RoomNumber, actualRreservation.RoomNumber);
    }
    
    [Fact]
    public async Task Reservations_Should_Not_Overlap()
    {
        // Arrange
        var client = _factory.CreateClient();

        var reservation1 = new Reservation
        {
            RoomNumber = "002",
            GuestEmail = "test@test.it",
            Start = DateTime.Now,
            End = DateTime.Now.AddDays(2),
        };
        var httpContent = new StringContent(JsonSerializer.Serialize(reservation1), Encoding.UTF8, MediaTypeNames.Application.Json);
        var response = await client.PostAsync("api/reservation", httpContent);
        var content = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, content);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var reservation2 = new Reservation
        {
            RoomNumber = "002",
            GuestEmail = "test2@test.it",
            Start = DateTime.Now,
            End = DateTime.Now.AddDays(2),
        };

        // Act
        httpContent = new StringContent(JsonSerializer.Serialize(reservation2), Encoding.UTF8, MediaTypeNames.Application.Json);
        response = await client.PostAsync("api/reservation", httpContent);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
