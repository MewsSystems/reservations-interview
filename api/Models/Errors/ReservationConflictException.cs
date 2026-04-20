namespace Models.Errors
{
    public class ReservationConflictException : Exception
    {
        public ReservationConflictException(string message)
            : base(message) { }
    }
}
