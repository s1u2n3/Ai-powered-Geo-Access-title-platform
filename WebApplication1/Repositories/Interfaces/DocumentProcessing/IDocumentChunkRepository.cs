namespace WebApplication1.Repositories.Interfaces.DocumentProcessing
{
    public interface IDocumentChunkRepository
    {        
    Task SaveChunksAsync(
        List<DocumentChunk> chunks);
    }
}
