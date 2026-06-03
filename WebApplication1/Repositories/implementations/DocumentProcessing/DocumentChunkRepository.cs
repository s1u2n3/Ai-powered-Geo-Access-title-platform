using Npgsql.PostgresTypes;

using WebApplication1.Repositories.Interfaces;
using WebApplication1.Repositories.Interfaces.DocumentProcessing;

namespace WebApplication1.Repositories.implementations.DocumentProcessing
{
    public class DocumentChunkRepository : IDocumentChunkRepository
    {
        private readonly PostgresDbContext _context;

        public DocumentChunkRepository(
          PostgresDbContext context)
        {
            _context = context;
        }
        
        public async Task SaveChunksAsync(
            List<DocumentChunk> chunks)
        {
            await _context
                .AddRangeAsync(chunks);

            await _context.SaveChangesAsync();
        }
    }
}
