using Pgvector;


public class DocumentChunk
{
    public Guid Id { get; set; }

    public string DocumentId { get; set; }

    public string ParcelId { get; set; }

    public string ChunkText { get; set; }

    public string ChunkType { get; set; }

    public int ChunkOrder { get; set; }

    public Vector Embedding { get; set; }
}
public class ChunkSearchResult
{
    public DocumentChunk Chunk { get; set; }

    public double Distance { get; set; }
}