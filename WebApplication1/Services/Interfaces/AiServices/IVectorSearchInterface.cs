namespace WebApplication1.Services.Interfaces.AiServices
{
    public interface IVectorSearchInterface
    {
        Task<List<ChunkSearchResult>>
            SearchAsync(
                string parcelId,
                string query,
                int topK = 20);
    }
}
