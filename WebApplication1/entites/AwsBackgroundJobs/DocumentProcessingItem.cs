namespace WebApplication1.entites.AwsBackgroundJobs
{
    public class DocumentProcessingItem
    {
        public Guid Id { get; set; }

        public Guid DocumentProcessingJobId { get; set; }

        public DocumentProcessingJob Job { get; set; }

        public string DocumentName { get; set; }

        public string S3Key { get; set; }

        public string Status { get; set; }

        public string? ErrorMessage { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? CompletedAt { get; set; }
    }
}
