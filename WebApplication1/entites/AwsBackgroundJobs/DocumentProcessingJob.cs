namespace WebApplication1.entites.AwsBackgroundJobs
{
    public class DocumentProcessingJob
    {
        public Guid Id { get; set; }

        public string ParcelId { get; set; }

        public string Status { get; set; }

        public int TotalDocuments { get; set; }

        public int ProcessedDocuments { get; set; }
        
        public DateTime CreatedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public ICollection<DocumentProcessingItem>
            Items
        { get; set; }
    }
}
