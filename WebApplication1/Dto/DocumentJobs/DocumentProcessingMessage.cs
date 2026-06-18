namespace WebApplication1.Dto.DocumentJobs
{
    public class DocumentProcessingMessage
    {
        public Guid JobId { get; set; }

        public Guid ItemId { get; set; }

        public string ParcelId { get; set; }

        public string S3Key { get; set; }
    }
}
