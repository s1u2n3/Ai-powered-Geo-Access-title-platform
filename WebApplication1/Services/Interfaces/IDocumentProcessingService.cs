using Amazon.S3.Model;
using WebApplication1.entites.AwsBackgroundJobs;

namespace WebApplication1.Services.Interfaces
{
    public interface IDocumentProcessingService
    {
        

    //Task ProcessParcelDocumentsAsync(
    //    List<S3Object> documents);

        Task<Guid> CreateDocumentProcessingJobAsync(string parcelId);

        Task ProcessSingleParcelDocument(string S3Key);

        Task<DocumentProcessingItem>GetDocumentProcessingItem(Guid itemId);
        Task MarkProcessingAsync(Guid itemId);

        Task MarkCompletedAsync(Guid itemId);

        Task MarkFailedAsync(Guid itemId,string error);

        Task UpdateJobProgressAsync(Guid jobId);


    }
}
