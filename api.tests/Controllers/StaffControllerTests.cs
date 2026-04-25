using System.Security.Claims;
using Controllers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Models;
using Moq;
using Repositories.Interfaces;
using Services;
using Xunit;

namespace api.tests.Controllers
{
    public class StaffControllerTests
    {
        private readonly Mock<IConfiguration> _configMock;
        private readonly Mock<IReservationRepository> _repoMock;
        private readonly Mock<ICheckInService> _checkInServiceMock;
        private readonly StaffController _controller;

        public StaffControllerTests()
        {
            _configMock = new Mock<IConfiguration>();
            _repoMock = new Mock<IReservationRepository>();
            _checkInServiceMock = new Mock<ICheckInService>();

            _controller = new StaffController(
                _configMock.Object, _repoMock.Object, checkInService: _checkInServiceMock.Object);

            // Mocking HttpContext for Authentication methods
            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        }

        [Fact]
        public async Task Login_WithCorrectCode_ReturnsOkAndSignsIn()
        {
            // Arrange
            _configMock.Setup(c => c.GetSection("staffAccessCode").Value).Returns("pass");

            var authServiceMock = new Mock<IAuthenticationService>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(s => s.GetService(typeof(IAuthenticationService)))
                .Returns(authServiceMock.Object);
            _controller.ControllerContext.HttpContext.RequestServices = serviceProviderMock.Object;

            // Act
            var result = await _controller.CheckCode("pass");

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetStaffReservations_ReturnsUpcomingReservations()
        {
            // Arrange
            var expectedReservations = new List<Reservation>
            {
                new Reservation
                {
                    Id = Guid.NewGuid(),
                    GuestEmail = "staff_view@test.com",
                    RoomNumber = "101",
                    Start = DateTime.Now.AddDays(1),
                    End = DateTime.Now.AddDays(2),
                },
            };
            _repoMock.Setup(r => r.GetUpcomingReservations()).ReturnsAsync(expectedReservations);

            // Act
            var result = await _controller.GetStaffReservations();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedReservations = Assert.IsAssignableFrom<IEnumerable<Reservation>>(
                okResult.Value
            );
            Assert.Single(returnedReservations);
            Assert.Contains(returnedReservations, r => r.GuestEmail == "staff_view@test.com");
        }

        [Fact]
        public async Task CheckIn_ServiceReturnsSuccess_ReturnsOk()
        {
            // Arrange
            var resId = Guid.NewGuid();
            _checkInServiceMock
                .Setup(s => s.ProcessCheckIn(resId, "test@test.com"))
                .ReturnsAsync((true, string.Empty));

            // Act
            var result = await _controller.CheckIn(resId, "test@test.com");

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task CheckIn_ServiceReturnsNotFoundError_ReturnsNotFound()
        {
            // Arrange
            var resId = Guid.NewGuid();
            _checkInServiceMock
                .Setup(s => s.ProcessCheckIn(resId, "test@test.com"))
                .ReturnsAsync((false, "Reservation not found."));

            // Act
            var result = await _controller.CheckIn(resId, "test@test.com");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
