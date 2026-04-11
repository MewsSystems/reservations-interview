namespace Models.Errors
{
    public class InvalidRoomNumber : Exception
    {
        public InvalidRoomNumber(string invalidRoomNumber)
            : base(
                $"Room number '{invalidRoomNumber}' is invalid. Expected format is ### with a floor 0-9 and door 01-99."
            ) { }
    }
}
