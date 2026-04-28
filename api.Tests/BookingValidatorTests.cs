using Models;
using Models.Errors;
using NUnit.Framework;
using Validators;

namespace api.Tests
{
    [TestFixture]
    public class BookingValidatorTests
    {
        // ── helpers ─────────────────────────────────────────────────────────────

        private static Reservation ValidBooking(
            string roomNumber = "101",
            string email = "guest@example.com",
            DateTime? start = null,
            DateTime? end = null
        )
        {
            var s = start ?? DateTime.Today;
            var e = end ?? DateTime.Today.AddDays(3);
            return new Reservation
            {
                Id = Guid.NewGuid(),
                RoomNumber = roomNumber,
                GuestEmail = email,
                Start = s,
                End = e,
            };
        }

        // ── valid room numbers ───────────────────────────────────────────────────

        [TestCase("101")]
        [TestCase("102")]
        [TestCase("103")]
        [TestCase("104")]
        [TestCase("105")]
        [TestCase("201")]
        [TestCase("202")]
        [TestCase("203")]
        [TestCase("901")]
        [TestCase("910")]
        public void Validate_ValidRoomNumber_DoesNotThrow(string roomNumber)
        {
            var booking = ValidBooking(roomNumber: roomNumber);
            Assert.DoesNotThrow(() => BookingValidator.Validate(booking));
        }

        // ── invalid room numbers ─────────────────────────────────────────────────

        [TestCase("000", TestName = "RoomNumber_000_AllZeros")]
        [TestCase("100", TestName = "RoomNumber_100_DoorIsZeroZero")]
        [TestCase("200", TestName = "RoomNumber_200_DoorIsZeroZero")]
        [TestCase("900", TestName = "RoomNumber_900_DoorIsZeroZero")]
        [TestCase("0",   TestName = "RoomNumber_SingleDigit")]
        [TestCase("1",   TestName = "RoomNumber_SingleDigit_1")]
        [TestCase("2020",TestName = "RoomNumber_FourDigits")]
        [TestCase("-101",TestName = "RoomNumber_Negative")]
        [TestCase("abc", TestName = "RoomNumber_NonNumeric")]
        [TestCase("",    TestName = "RoomNumber_Empty")]
        public void Validate_InvalidRoomNumber_ThrowsInvalidBooking(string roomNumber)
        {
            var booking = ValidBooking(roomNumber: roomNumber);
            Assert.Throws<InvalidBooking>(() => BookingValidator.Validate(booking));
        }

        // ── email validation ─────────────────────────────────────────────────────

        [TestCase("guest@example.com")]
        [TestCase("a@b.io")]
        [TestCase("first.last+tag@sub.domain.org")]
        public void Validate_ValidEmail_DoesNotThrow(string email)
        {
            var booking = ValidBooking(email: email);
            Assert.DoesNotThrow(() => BookingValidator.Validate(booking));
        }

        [TestCase("notanemail",       TestName = "Email_NoAtSign")]
        [TestCase("missing@domain",   TestName = "Email_NoDotInDomain")]
        [TestCase("@nodomain.com",    TestName = "Email_EmptyLocalPart")]
        [TestCase("",                 TestName = "Email_Empty")]
        [TestCase("spaces @a.com",    TestName = "Email_SpaceInLocal")]
        public void Validate_InvalidEmail_ThrowsInvalidBooking(string email)
        {
            var booking = ValidBooking(email: email);
            Assert.Throws<InvalidBooking>(() => BookingValidator.Validate(booking));
        }

        // ── date range validation ────────────────────────────────────────────────

        [TestCase(1,  TestName = "Duration_1Day_IsValid")]
        [TestCase(15, TestName = "Duration_15Days_IsValid")]
        [TestCase(30, TestName = "Duration_30Days_IsValid")]
        public void Validate_ValidDuration_DoesNotThrow(int days)
        {
            var start = DateTime.Today;
            var booking = ValidBooking(start: start, end: start.AddDays(days));
            Assert.DoesNotThrow(() => BookingValidator.Validate(booking));
        }

        [TestCase(0,  TestName = "Duration_SameDay_StartEqualsEnd")]
        [TestCase(-1, TestName = "Duration_EndBeforeStart")]
        public void Validate_StartNotBeforeEnd_ThrowsInvalidBooking(int daysOffset)
        {
            var start = DateTime.Today;
            var booking = ValidBooking(start: start, end: start.AddDays(daysOffset));
            Assert.Throws<InvalidBooking>(() => BookingValidator.Validate(booking));
        }

        [TestCase(31, TestName = "Duration_31Days_ExceedsMaximum")]
        [TestCase(60, TestName = "Duration_60Days_ExceedsMaximum")]
        public void Validate_DurationExceedsMaximum_ThrowsInvalidBooking(int days)
        {
            var start = DateTime.Today;
            var booking = ValidBooking(start: start, end: start.AddDays(days));
            Assert.Throws<InvalidBooking>(() => BookingValidator.Validate(booking));
        }
    }
}
