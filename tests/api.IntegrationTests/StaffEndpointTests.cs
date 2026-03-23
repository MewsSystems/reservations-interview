using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Contracts;
using Models;

namespace api.IntegrationTests;

public class StaffEndpointTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public StaffEndpointTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    #region Helpers

    private async Task<string> GetStaffToken()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/staff/login");
        request.Headers.Add("X-Staff-Code", "pass");

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("token").GetString()!;
    }

    private async Task<HttpClient> GetAuthenticatedClient()
    {
        var token = await GetStaffToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return _client;
    }

    private static MultipartFormDataContent CreateCsvFile(string content, string fileName = "rooms.csv")
    {
        var formData = new MultipartFormDataContent();
        var fileContent = new StringContent(content);
        formData.Add(fileContent, "file", fileName);
        return formData;
    }

    #endregion

    #region Auth

    [Fact]
    public async Task Login_with_valid_code_returns_token()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/staff/login");
        request.Headers.Add("X-Staff-Code", "pass");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.TryGetProperty("token", out var token));
        Assert.False(string.IsNullOrEmpty(token.GetString()));
    }

    [Fact]
    public async Task Login_with_invalid_code_returns_401()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/staff/login");
        request.Headers.Add("X-Staff-Code", "wrong");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Get_reservations_without_token_returns_401()
    {
        var client = _client;
        client.DefaultRequestHeaders.Authorization = null;

        var response = await client.GetAsync("/api/reservations");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Delete_reservation_without_token_returns_401()
    {
        var client = _client;
        client.DefaultRequestHeaders.Authorization = null;

        var response = await client.DeleteAsync($"/api/reservations/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Reservations

    [Fact]
    public async Task Get_reservations_with_token_returns_200()
    {
        var client = await GetAuthenticatedClient();

        var response = await client.GetAsync("/api/reservations");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Get_reservations_filtered_by_from_date()
    {
        var client = await GetAuthenticatedClient();

        var booking = new ReservationRequest
        {
            RoomNumber = "104",
            GuestEmail = "staff-test@mjail.com",
            Start = DateTime.Today.AddDays(50),
            End = DateTime.Today.AddDays(53)
        };
        var createResponse = await client.PostAsJsonAsync("/api/reservations", booking);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        // should include our booking
        var from = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
        var response = await client.GetAsync($"/api/reservations?from={from}");
        var reservations = await response.Content.ReadFromJsonAsync<List<Reservation>>(_jsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(reservations);
        Assert.Contains(reservations, r => r.RoomNumber == "104");
    }

    [Fact]
    public async Task Get_reservations_filtered_by_room_number()
    {
        var client = await GetAuthenticatedClient();

        var booking = new ReservationRequest
        {
            RoomNumber = "105",
            GuestEmail = "room-filter@mjail.com",
            Start = DateTime.Today.AddDays(60),
            End = DateTime.Today.AddDays(63)
        };
        await client.PostAsJsonAsync("/api/reservations", booking);

        var response = await client.GetAsync("/api/reservations?roomNumber=105");
        var reservations = await response.Content.ReadFromJsonAsync<List<Reservation>>(_jsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(reservations);
        Assert.All(reservations, r => Assert.Equal("105", r.RoomNumber));
    }

    [Fact]
    public async Task Get_reservations_filtered_by_guest_email()
    {
        var client = await GetAuthenticatedClient();

        var booking = new ReservationRequest
        {
            RoomNumber = "201",
            GuestEmail = "email-filter@mjail.com",
            Start = DateTime.Today.AddDays(70),
            End = DateTime.Today.AddDays(73)
        };
        await client.PostAsJsonAsync("/api/reservations", booking);

        var response = await client.GetAsync("/api/reservations?guestEmail=email-filter@mjail.com");
        var reservations = await response.Content.ReadFromJsonAsync<List<Reservation>>(_jsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(reservations);
        Assert.All(reservations, r => Assert.Equal("email-filter@mjail.com", r.GuestEmail));
    }

    #endregion

    #region Check-in

    [Fact]
    public async Task CheckIn_today_reservation_returns_200_and_marks_checked_in()
    {
        var client = await GetAuthenticatedClient();

        var booking = new ReservationRequest
        {
            RoomNumber = "104",
            GuestEmail = "checkin-test@mjail.com",
            Start = DateTime.Today,
            End = DateTime.Today.AddDays(3)
        };
        var createResponse = await client.PostAsJsonAsync("/api/reservations", booking);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<Reservation>(_jsonOptions);
        Assert.NotNull(created);

        // Check in
        var checkInResponse = await client.PostAsJsonAsync(
            $"/api/reservations/{created.Id}/check-in",
            new { guestEmail = "checkin-test@mjail.com" });

        Assert.Equal(HttpStatusCode.OK, checkInResponse.StatusCode);
        var checkedIn = await checkInResponse.Content.ReadFromJsonAsync<Reservation>(_jsonOptions);
        Assert.NotNull(checkedIn);
        Assert.True(checkedIn.CheckedIn);

        // Verify room is marked as occupied
        var roomResponse = await client.GetAsync($"/api/rooms/{created.RoomNumber}");
        var room = await roomResponse.Content.ReadFromJsonAsync<Room>(_jsonOptions);
        Assert.NotNull(room);
        Assert.Equal((int)State.Occupied, (int)room.State);
    }

    [Fact]
    public async Task CheckIn_with_wrong_email_returns_400()
    {
        var client = await GetAuthenticatedClient();

        var booking = new ReservationRequest
        {
            RoomNumber = "105",
            GuestEmail = "correct@mjail.com",
            Start = DateTime.Today,
            End = DateTime.Today.AddDays(2)
        };
        var createResponse = await client.PostAsJsonAsync("/api/reservations", booking);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<Reservation>(_jsonOptions);
        Assert.NotNull(created);

        // Check in with wrong email
        var checkInResponse = await client.PostAsJsonAsync(
            $"/api/reservations/{created.Id}/check-in",
            new { guestEmail = "wrong@mjail.com" });

        Assert.Equal(HttpStatusCode.BadRequest, checkInResponse.StatusCode);
        var error = await checkInResponse.Content.ReadFromJsonAsync<ErrorResponse>(_jsonOptions);
        Assert.NotNull(error);
        Assert.Contains("email", error.Detail, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CheckIn_dirty_room_returns_400()
    {
        var client = await GetAuthenticatedClient();

        // Create the room first
        await client.PostAsJsonAsync("/api/rooms", new { number = "501", state = 0 });

        var booking1 = new ReservationRequest
        {
            RoomNumber = "501",
            GuestEmail = "first@mjail.com",
            Start = DateTime.Today,
            End = DateTime.Today.AddDays(1)
        };
        var create1 = await client.PostAsJsonAsync("/api/reservations", booking1);
        var res1 = await create1.Content.ReadFromJsonAsync<Reservation>(_jsonOptions);
        Assert.NotNull(res1);

        await client.PostAsJsonAsync($"/api/reservations/{res1.Id}/check-in",
            new { guestEmail = "first@mjail.com" });

        // Check out to mark room dirty
        await client.PostAsJsonAsync($"/api/reservations/{res1.Id}/check-out",
            new { guestEmail = "first@mjail.com" });

        await client.DeleteAsync($"/api/reservations/{res1.Id}");

        // Try to check in another reservation 
        var booking2 = new ReservationRequest
        {
            RoomNumber = "501",
            GuestEmail = "second@mjail.com",
            Start = DateTime.Today,
            End = DateTime.Today.AddDays(3)
        };
        var create2 = await client.PostAsJsonAsync("/api/reservations", booking2);
        var res2 = await create2.Content.ReadFromJsonAsync<Reservation>(_jsonOptions);
        Assert.NotNull(res2);

        var checkInResponse = await client.PostAsJsonAsync(
            $"/api/reservations/{res2.Id}/check-in",
            new { guestEmail = "second@mjail.com" });

        Assert.Equal(HttpStatusCode.BadRequest, checkInResponse.StatusCode);
        var error = await checkInResponse.Content.ReadFromJsonAsync<ErrorResponse>(_jsonOptions);
        Assert.NotNull(error);
        Assert.Contains("dirty", error.Detail, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Check-out

    [Fact]
    public async Task CheckOut_checked_in_reservation_returns_200_and_marks_room_dirty()
    {
        var client = await GetAuthenticatedClient();

        await client.PostAsJsonAsync("/api/rooms", new { number = "601", state = 0 });

        var booking = new ReservationRequest
        {
            RoomNumber = "601",
            GuestEmail = "checkout@mjail.com",
            Start = DateTime.Today,
            End = DateTime.Today.AddDays(2)
        };
        var createResponse = await client.PostAsJsonAsync("/api/reservations", booking);
        var created = await createResponse.Content.ReadFromJsonAsync<Reservation>(_jsonOptions);
        Assert.NotNull(created);

        await client.PostAsJsonAsync($"/api/reservations/{created.Id}/check-in",
            new { guestEmail = "checkout@mjail.com" });

        var checkOutResponse = await client.PostAsJsonAsync(
            $"/api/reservations/{created.Id}/check-out",
            new { guestEmail = "checkout@mjail.com" });

        Assert.Equal(HttpStatusCode.OK, checkOutResponse.StatusCode);
        var result = await checkOutResponse.Content.ReadFromJsonAsync<Reservation>(_jsonOptions);
        Assert.NotNull(result);
        Assert.True(result.CheckedOut);
        Assert.NotNull(result.CheckedOutAt);

        var roomResponse = await client.GetFromJsonAsync<JsonElement>("/api/rooms/601");
        Assert.Equal(2, roomResponse.GetProperty("state").GetInt32()); // Dirty
    }

    [Fact]
    public async Task CheckOut_without_check_in_returns_400()
    {
        var client = await GetAuthenticatedClient();

        await client.PostAsJsonAsync("/api/rooms", new { number = "602", state = 0 });

        var booking = new ReservationRequest
        {
            RoomNumber = "602",
            GuestEmail = "notin@mjail.com",
            Start = DateTime.Today,
            End = DateTime.Today.AddDays(2)
        };
        var createResponse = await client.PostAsJsonAsync("/api/reservations", booking);
        var created = await createResponse.Content.ReadFromJsonAsync<Reservation>(_jsonOptions);
        Assert.NotNull(created);

        var checkOutResponse = await client.PostAsJsonAsync(
            $"/api/reservations/{created.Id}/check-out",
            new { guestEmail = "notin@mjail.com" });

        Assert.Equal(HttpStatusCode.BadRequest, checkOutResponse.StatusCode);
    }

    [Fact]
    public async Task CheckOut_already_checked_out_returns_409()
    {
        var client = await GetAuthenticatedClient();

        await client.PostAsJsonAsync("/api/rooms", new { number = "603", state = 0 });

        var booking = new ReservationRequest
        {
            RoomNumber = "603",
            GuestEmail = "double-out@mjail.com",
            Start = DateTime.Today,
            End = DateTime.Today.AddDays(2)
        };
        var createResponse = await client.PostAsJsonAsync("/api/reservations", booking);
        var created = await createResponse.Content.ReadFromJsonAsync<Reservation>(_jsonOptions);
        Assert.NotNull(created);

        await client.PostAsJsonAsync($"/api/reservations/{created.Id}/check-in",
            new { guestEmail = "double-out@mjail.com" });
        await client.PostAsJsonAsync($"/api/reservations/{created!.Id}/check-out",
            new { guestEmail = "double-out@mjail.com" });

        var secondCheckOut = await client.PostAsJsonAsync(
            $"/api/reservations/{created.Id}/check-out",
            new { guestEmail = "double-out@mjail.com" });

        Assert.Equal(HttpStatusCode.Conflict, secondCheckOut.StatusCode);
    }

    #endregion

    #region Import

    [Fact]
    public async Task Import_valid_csv_returns_200_with_results()
    {
        var client = await GetAuthenticatedClient();
        var csv = "number\n301\n302\n303";

        var response = await client.PostAsync("/api/rooms/import", CreateCsvFile(csv));
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(3, result.GetProperty("imported").GetInt32());
        Assert.Equal(0, result.GetProperty("failed").GetInt32());
    }

    [Fact]
    public async Task Import_csv_with_invalid_rooms_returns_errors()
    {
        var client = await GetAuthenticatedClient();
        var csv = "number\n401\n000\nabc\n402";

        var response = await client.PostAsync("/api/rooms/import", CreateCsvFile(csv));
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, result.GetProperty("imported").GetInt32());
        Assert.Equal(2, result.GetProperty("failed").GetInt32());

        var errors = result.GetProperty("errors");
        Assert.Equal(2, errors.GetArrayLength());
    }

    #endregion

    #region Room Status

    [Fact]
    public async Task Patch_room_state_to_ready_returns_200()
    {
        var client = await GetAuthenticatedClient();

        await client.PostAsJsonAsync("/api/rooms", new { number = "701", state = 0 });

        var request = new HttpRequestMessage(HttpMethod.Patch, "/api/rooms/701")
        {
            Content = JsonContent.Create(new { state = 2 }) // Dirty
        };
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var room = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(2, room.GetProperty("state").GetInt32());

        // Now mark as ready
        var request2 = new HttpRequestMessage(HttpMethod.Patch, "/api/rooms/701")
        {
            Content = JsonContent.Create(new { state = 0 })
        };
        var response2 = await client.SendAsync(request2);

        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        var room2 = await response2.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(0, room2.GetProperty("state").GetInt32());
    }

    [Fact]
    public async Task Patch_nonexistent_room_returns_404()
    {
        var client = await GetAuthenticatedClient();

        var request = new HttpRequestMessage(HttpMethod.Patch, "/api/rooms/999")
        {
            Content = JsonContent.Create(new { state = 0 })
        };
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion
}
