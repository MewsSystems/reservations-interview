namespace Models.Validation
{
    public class ValidationResult
    {
        public bool IsValid { get; }
        public string ErrorMessage { get; }

        public ValidationResult(bool isValid, string errorMessage = "")
        {
            IsValid = isValid;
            ErrorMessage = errorMessage ?? string.Empty;
        }

        public static ValidationResult Valid() => new ValidationResult(true);

        public static ValidationResult Invalid(string errorMessage) => new ValidationResult(false, errorMessage);
    }
}
