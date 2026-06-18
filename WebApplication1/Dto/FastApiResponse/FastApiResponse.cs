using Pgvector;

namespace WebApplication1.Dto.FastApiResponse;
public class FastApiResponse
{
    public string DocumentId { get; set; }

    public string DocumentType { get; set; }

    public List<FastApiChunk> Chunks { get; set; }
}

public class FastApiChunk
{
    public string ChunkId { get; set; }

    public int ChunkOrder { get; set; }

    public string Text { get; set; }

    public float[] Embedding { get; set; }

    public FastApiMetadata Metadata { get; set; }
}

public class FastApiMetadata
{
    public string Section { get; set; }

    public int CharCount { get; set; }
}
