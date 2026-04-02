namespace Models
{
    public class ImportOptions
    {
        public long MaxFileSizeBytes { get; set; } = 102_400;
        
        public long MaxRows { get; set; } = 500;
    }
}
