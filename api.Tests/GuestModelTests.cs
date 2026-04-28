using Models;
using NUnit.Framework;

namespace api.Tests
{
    [TestFixture]
    public class GuestModelTests
    {
        // ── GetLastName ──────────────────────────────────────────────────────────

        [Test]
        public void GetLastName_WithSurname_ReturnsSurname()
        {
            var guest = new Guest { Email = "a@b.com", Name = "Alice Smith", Surname = "Smith" };

            Assert.That(guest.GetLastName(), Is.EqualTo("Smith"));
        }

        [Test]
        public void GetLastName_NullSurname_ReturnsFullName()
        {
            var guest = new Guest { Email = "a@b.com", Name = "Alice Smith", Surname = null };

            Assert.That(guest.GetLastName(), Is.EqualTo("Alice Smith"));
        }

        [Test]
        public void GetLastName_EmptySurname_ReturnsEmptySurname()
        {
            var guest = new Guest { Email = "a@b.com", Name = "Alice", Surname = "" };

            // Empty string is not null, so it should be returned as-is
            Assert.That(guest.GetLastName(), Is.EqualTo(""));
        }
    }
}
