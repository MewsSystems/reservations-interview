using Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Xunit;
using api.Tests.Common;

namespace api.Tests;

public sealed class StaffControllerTests
{
    [Fact]
    public async Task CheckCode_AuthenticatesWhenSharedAccessCodeMatches()
    {
        await using var fixture = await TestDb.CreateAsync();
        var controller = CreateController(fixture, "pass");

        var result = controller.CheckCode("pass");

        Assert.IsType<NoContentResult>(result);
        Assert.Contains("access=1", controller.Response.Headers["Set-Cookie"].ToString());
    }

    [Fact]
    public async Task CheckCode_RejectsInvalidSharedAccessCode()
    {
        await using var fixture = await TestDb.CreateAsync();
        var controller = CreateController(fixture, "pass");

        var result = controller.CheckCode("wrong");

        Assert.IsType<UnauthorizedResult>(result);
        Assert.Empty(controller.Response.Headers["Set-Cookie"].ToString());
    }

    private static StaffController CreateController(TestDb fixture, string accessCode)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?> { ["staffAccessCode"] = accessCode }
            )
            .Build();

        var controller = new StaffController(config, fixture.ReservationRepository)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        return controller;
    }
}
