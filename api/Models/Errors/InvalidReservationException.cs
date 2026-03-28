namespace Models.Errors
{
    public class InvalidReservationException : Exception
    {
        public InvalidReservationException(string message)
            : base(message) { }
    }
}
