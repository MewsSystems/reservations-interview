namespace Models.Errors
{
    public class InvalidCheckInException : Exception
    {
        public InvalidCheckInException(string message)
            : base(message) { }
    }
}
