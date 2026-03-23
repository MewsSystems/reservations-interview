namespace Contracts
{
    public record ImportResult(
        int TotalRows,
        int Imported,
        int Failed,
        List<ImportError> Errors
    );

    public record ImportError(int Line, string RoomNumber, string Message);
}
