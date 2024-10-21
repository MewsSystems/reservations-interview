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
        var actualRreservation = JsonSerializer.Deserialize<Reservation>(content, _options);

        // Assert
        Assert.True(response.IsSuccessStatusCode, content);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(actualRreservation);
        Assert.Equal(expectedReservation.GuestEmail, actualRreservation.GuestEmail);
        Assert.Equal(expectedReservation.RoomNumber, actualRreservation.RoomNumber);
    }
}
