using Amazon.S3.Model;

namespace WebApplication1.Services.Interfaces
{
    public interface IDocumentProcessingService
    {
        
    Task ProcessParcelDocumentsAsync(
        List<S3Object> documents);
    
}
}
