using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Contracts;
using Models;

namespace api.IntegrationTests;

/// <summary>
/// Tests run against in-memory sqlite. Room numbers (existing) should differ along tests to avoid tests conflicts. 
/// </summary>
public class ReservationsEndpointTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ReservationsEndpointTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }


    [Fact]
    public async Task Post_valid_reservation_returns_201()
    {
        var booking = new ReservationRequest
        {
            RoomNumber = "101",
            GuestEmail = "guest@mjail.com",
            Start = DateTime.Today.AddDays(1),
            End = DateTime.Today.AddDays(3)
        };

        var response = await _client.PostAsJsonAsync("/api/reservations", booking);

        Assert.True(
            response.StatusCode == HttpStatusCode.Created,
            $"Expected 201 but got {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}"
        );

        var reservation = await response.Content.ReadFromJsonAsync<Reservation>(_jsonOptions);
        Assert.NotNull(reservation);
        Assert.NotEqual(Guid.Empty, reservation.Id);
        Assert.Equal("101", reservation.RoomNumber);
        Assert.Equal("guest@mjail.com", reservation.GuestEmail);
    }

    [Fact]
    public async Task Get_nonexistent_reservation_returns_404()
    {
        var fakeId = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/reservations/{fakeId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(_jsonOptions);
        Assert.NotNull(error);
        Assert.Equal("NotFound", error.Title);
        Assert.Equal("Reservation", error.ResourceType);
    }


    [Fact]
    public async Task Post_reservation_with_nonexistent_room_returns_404()
    {
        var booking = new ReservationRequest
        {
            RoomNumber = "999",
            GuestEmail = "guest@mjail.com",
            Start = DateTime.Today.AddDays(1),
            End = DateTime.Today.AddDays(3)
        };

        var response = await _client.PostAsJsonAsync("/api/reservations", booking);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(_jsonOptions);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.NotNull(error);
        Assert.Equal("NotFound", error.Title);
        Assert.Equal("Room", error.ResourceType);
        Assert.Equal("999", error.ResourceId);
    }

    [Fact]
    public async Task Post_reservation_with_invalid_data_returns_400_with_errors()
    {
        var invalidBooking = new ReservationRequest
        {
            RoomNumber = "000",
            GuestEmail = "not-an-email",
            Start = DateTime.Today.AddDays(-1),
            End = DateTime.Today.AddDays(-1)
        };

        var response = await _client.PostAsJsonAsync("/api/reservations", invalidBooking);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(_jsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(error);
        Assert.Equal("BadRequest", error.Title);
        Assert.NotNull(error.Errors);
        Assert.True(error.Errors.ContainsKey("RoomNumber"));
        Assert.True(error.Errors.ContainsKey("GuestEmail"));
        Assert.True(error.Errors.ContainsKey("Start"));
    }

    [Fact]
    public async Task Post_reservation_with_invalid_room_number_returns_room_error()
    {
        var booking = new ReservationRequest
        {
            RoomNumber = "abc",
            GuestEmail = "guest@mjail.com",
            Start = DateTime.Today.AddDays(1),
            End = DateTime.Today.AddDays(3)
        };

        var response = await _client.PostAsJsonAsync("/api/reservations", booking);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(_jsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(error?.Errors);
        Assert.True(error.Errors.ContainsKey("RoomNumber"));
        Assert.False(error.Errors.ContainsKey("GuestEmail"));
    }

    [Fact]
    public async Task Post_reservation_with_invalid_email_returns_email_error()
    {
        var booking = new ReservationRequest
        {
            RoomNumber = "101",
            GuestEmail = "nodomain",
            Start = DateTime.Today.AddDays(1),
            End = DateTime.Today.AddDays(3)
        };

        var response = await _client.PostAsJsonAsync("/api/reservations", booking);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(_jsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(error?.Errors);
        Assert.True(error.Errors.ContainsKey("GuestEmail"));
        Assert.False(error.Errors.ContainsKey("RoomNumber"));
    }

    [Fact]
    public async Task Post_double_booking_same_dates_returns_409()
    {
        var booking = new ReservationRequest
        {
            RoomNumber = "201",
            GuestEmail = "guest@mjail.com",
            Start = DateTime.Today.AddDays(10),
            End = DateTime.Today.AddDays(13)
        };

        var first = await _client.PostAsJsonAsync("/api/reservations", booking);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var second = await _client.PostAsJsonAsync("/api/reservations", booking);
        var error = await second.Content.ReadFromJsonAsync<ErrorResponse>(_jsonOptions);

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
        Assert.NotNull(error);
        Assert.Equal("Conflict", error.Title);
        Assert.Equal("Reservation", error.ResourceType);
        Assert.Contains("201", error.Detail);
    }

    [Fact]
    public async Task Post_overlapping_booking_returns_409()
    {
        var first = new ReservationRequest
        {
            RoomNumber = "202",
            GuestEmail = "guest@mjail.com",
            Start = DateTime.Today.AddDays(10),
            End = DateTime.Today.AddDays(15)
        };

        var firstResponse = await _client.PostAsJsonAsync("/api/reservations", first);
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        var overlapping = new ReservationRequest
        {
            RoomNumber = "202",
            GuestEmail = "other@mjail.com",
            Start = DateTime.Today.AddDays(13),
            End = DateTime.Today.AddDays(18)
        };

        var secondResponse = await _client.PostAsJsonAsync("/api/reservations", overlapping);
        var error = await secondResponse.Content.ReadFromJsonAsync<ErrorResponse>(_jsonOptions);

        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
        Assert.NotNull(error);
        Assert.Equal("Conflict", error.Title);
    }

    [Fact]
    public async Task Post_non_overlapping_booking_same_room_succeeds()
    {
        var first = new ReservationRequest
        {
            RoomNumber = "203",
            GuestEmail = "guest@mjail.com",
            Start = DateTime.Today.AddDays(10),
            End = DateTime.Today.AddDays(13)
        };

        var firstResponse = await _client.PostAsJsonAsync("/api/reservations", first);
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        var nonOverlapping = new ReservationRequest
        {
            RoomNumber = "203",
            GuestEmail = "other@mjail.com",
            Start = DateTime.Today.AddDays(13),
            End = DateTime.Today.AddDays(16)
        };

        var secondResponse = await _client.PostAsJsonAsync("/api/reservations", nonOverlapping);
        Assert.Equal(HttpStatusCode.Created, secondResponse.StatusCode);
    }

    [Fact]
    public async Task Post_concurrent_double_booking_same_dates_returns_409()
    {
        var booking = new ReservationRequest
        {
            RoomNumber = "103",
            GuestEmail = "guest@mjail.com",
            Start = DateTime.Today.AddDays(10),
            End = DateTime.Today.AddDays(13)
        };

        var task1 = _client.PostAsJsonAsync("/api/reservations", booking);
        var task2 = _client.PostAsJsonAsync("/api/reservations", booking);
        var task3 = _client.PostAsJsonAsync("/api/reservations", booking);
        var task4 = _client.PostAsJsonAsync("/api/reservations", booking);
        
        var responses = await Task.WhenAll(task1, task2, task3, task4);

        var statuses = responses.Select(r => r.StatusCode).ToList();
        Assert.Equal(1, statuses.Count(s => s == HttpStatusCode.Created));
        Assert.Equal(3, statuses.Count(s => s == HttpStatusCode.Conflict));
    }
}
