using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace api.IntegrationTests;

public class ExceptionHandlingTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ExceptionHandlingTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Returns_404_with_error_body_when_resource_not_found()
    {
        var fakeId = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/reservations/{fakeId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var body = await DeserializeResponse(response);
        Assert.Equal("Reservation", body.ResourceType);
        Assert.Equal(fakeId.ToString(), body.ResourceId);
        Assert.Equal("NotFound", body.Title);
        Assert.False(string.IsNullOrEmpty(body.Detail));
    }

    [Fact]
    public async Task Returns_400_with_error_body_when_validation_fails()
    {
        var response = await _client.GetAsync("/api/rooms/abc");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var body = await DeserializeResponse(response);
        Assert.Equal("Room", body.ResourceType);
        Assert.Equal("abc", body.ResourceId);
        Assert.Equal("BadRequest", body.Title);
        Assert.Contains("not a valid room number", body.Detail);
    }

    [Fact]
    public async Task Returns_json_content_type_for_error_responses()
    {
        var fakeId = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/reservations/{fakeId}");

        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Error_response_contains_all_expected_fields()
    {
        var fakeId = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/reservations/{fakeId}");
        var content = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;

        Assert.True(root.TryGetProperty("resourceType", out _));
        Assert.True(root.TryGetProperty("resourceId", out _));
        Assert.True(root.TryGetProperty("detail", out _));
        Assert.True(root.TryGetProperty("title", out _));
    }

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static async Task<ErrorResponse> DeserializeResponse(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ErrorResponse>(content, _jsonOptions)
            ?? throw new Exception("Failed to deserialize error response");
    }

    private record ErrorResponse(string ResourceType, string ResourceId, string Detail, string Title);
}
