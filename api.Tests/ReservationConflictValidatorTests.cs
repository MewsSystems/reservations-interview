using Models;
using Models.Errors;
using NUnit.Framework;
using Validators;

namespace api.Tests
{
    [TestFixture]
    public class ReservationConflictValidatorTests
    {
        private static Reservation MakeReservation() => new()
        {
            Id = Guid.NewGuid(),
            RoomNumber = "101",
            GuestEmail = "guest@example.com",
            Start = DateTime.Today,
            End = DateTime.Today.AddDays(3),
        };

        [Test]
        public void ValidateNoConflict_NoConflict_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
                ReservationConflictValidator.ValidateNoConflict(MakeReservation(), hasConflict: false));
        }

        [Test]
        public void ValidateNoConflict_HasConflict_ThrowsConflictException()
        {
            Assert.Throws<ConflictException>(() =>
                ReservationConflictValidator.ValidateNoConflict(MakeReservation(), hasConflict: true));
        }

        [Test]
        public void ValidateNoConflict_HasConflict_MessageContainsRoomNumber()
        {
            var ex = Assert.Throws<ConflictException>(() =>
                ReservationConflictValidator.ValidateNoConflict(MakeReservation(), hasConflict: true));

            Assert.That(ex!.Message, Does.Contain("101"));
        }
    }
}
