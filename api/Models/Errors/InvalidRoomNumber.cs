namespace Models.Errors
{
    public class InvalidRoomNumber : Exception
    {
        public InvalidRoomNumber(string invalidRoomNumber)
            : base($"The value ${invalidRoomNumber} is not a valid") { }
    }
}
