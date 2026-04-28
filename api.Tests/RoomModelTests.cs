using Models;
using Models.Errors;
using NUnit.Framework;

namespace api.Tests
{
    [TestFixture]
    public class RoomModelTests
    {
        // ── FormatRoomNumber ─────────────────────────────────────────────────────

        [TestCase(101, "101", TestName = "FormatRoomNumber_101_ReturnsString101")]
        [TestCase(1,   "001", TestName = "FormatRoomNumber_1_PadsToThreeDigits")]
        [TestCase(10,  "010", TestName = "FormatRoomNumber_10_PadsToThreeDigits")]
        [TestCase(999, "999", TestName = "FormatRoomNumber_999_NoPadNeeded")]
        public void FormatRoomNumber_ReturnsZeroPaddedThreeDigitString(int input, string expected)
        {
            Assert.That(Room.FormatRoomNumber(input), Is.EqualTo(expected));
        }

        // ── IsValidRoomNumber ────────────────────────────────────────────────────
        // (comprehensive cases are in BookingValidatorTests; these cover the
        //  boundary cases of the static method directly)

        [TestCase("101", true,  TestName = "IsValidRoomNumber_101_IsValid")]
        [TestCase("100", false, TestName = "IsValidRoomNumber_100_EndsInDoubleZero")]
        [TestCase("001", true,  TestName = "IsValidRoomNumber_001_IsValid")]
        [TestCase("999", true,  TestName = "IsValidRoomNumber_999_IsValid")]
        [TestCase("1000",false, TestName = "IsValidRoomNumber_FourDigits_IsInvalid")]
        [TestCase("10",  false, TestName = "IsValidRoomNumber_TwoDigits_IsInvalid")]
        [TestCase("abc", false, TestName = "IsValidRoomNumber_NonNumeric_IsInvalid")]
        [TestCase("",    false, TestName = "IsValidRoomNumber_Empty_IsInvalid")]
        public void IsValidRoomNumber_ReturnsExpected(string roomNumber, bool expected)
        {
            Assert.That(Room.IsValidRoomNumber(roomNumber), Is.EqualTo(expected));
        }

        // ── ConvertRoomNumberToInt ───────────────────────────────────────────────

        [TestCase("101", 101, TestName = "ConvertRoomNumberToInt_101_Returns101")]
        [TestCase("001", 1,   TestName = "ConvertRoomNumberToInt_001_Returns1")]
        [TestCase("999", 999, TestName = "ConvertRoomNumberToInt_999_Returns999")]
        public void ConvertRoomNumberToInt_ValidNumber_ReturnsInt(string input, int expected)
        {
            Assert.That(Room.ConvertRoomNumberToInt(input), Is.EqualTo(expected));
        }

        [TestCase("abc", TestName = "ConvertRoomNumberToInt_NonNumeric_ThrowsInvalidRoomNumber")]
        [TestCase("",    TestName = "ConvertRoomNumberToInt_Empty_ThrowsInvalidRoomNumber")]
        public void ConvertRoomNumberToInt_InvalidInput_ThrowsInvalidRoomNumber(string input)
        {
            Assert.Throws<InvalidRoomNumber>(() => Room.ConvertRoomNumberToInt(input));
        }
    }
}
