using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using WebApplication1.Services.Implementaions;
using WebApplication1.Services.Interfaces.AiServices;

public class VectorSearchService
    : IVectorSearchInterface
{
    private readonly
        PostgresDbContext _context;

    private readonly
        EmbeddingsService _embeddingsService;

    public VectorSearchService(
        PostgresDbContext context,
        EmbeddingsService embeddingsService)
    {
        _context = context;
        _embeddingsService =
            embeddingsService;
    }

    public async Task<List<ChunkSearchResult>> SearchAsync(
    string parcelId,
    string query,
    int topK = 10)
    {
        var queryEmbedding =
            await _embeddingsService
                .GenerateEmbeddingAsync(query);

        return await _context.DocumentChunks
            .Where(x => x.ParcelId == parcelId)
            .Select(x => new ChunkSearchResult
            {
                Chunk = x,
                Distance = x.Embedding.CosineDistance(queryEmbedding)
            }).Where(x => x.Distance < 0.35)
            .OrderBy(x => x.Distance)
            .Take(topK)
            .ToListAsync();
    }
}