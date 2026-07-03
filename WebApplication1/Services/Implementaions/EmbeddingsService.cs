
using System.Net.Http.Json;
using Pgvector;

namespace WebApplication1.Services.Implementaions
{


    public class EmbeddingsService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        public EmbeddingsService(HttpClient httpClient, IConfiguration configuration)
        {

            _httpClient = httpClient;
            _configuration = configuration;
        }


        //public  async Task<Vector> GenerateEmbeddingAsync(string text)
        // {
        //    var request = new
        //    {
        //        model = "nomic-embed-text",
        //        prompt = text
        //    };

        //    var response =
        //        await _httpClient
        //            .PostAsJsonAsync(
        //                "http://localhost:11434/api/embeddings",
        //                request);

        //    response
        //        .EnsureSuccessStatusCode();

        //    var result =
        //        await response.Content
        //            .ReadFromJsonAsync<
        //                OllamaEmbeddingResponse>();

        //    return new Vector(
        //        result.embedding);

        // }

        public async Task<Vector> GenerateEmbeddingAsync(string text)
        {
            var apiKey = _configuration["Gemini:ApiKey"];

            var request = new
            {
                model = "models/embedding-001",
                content = new
                {
                    parts = new[]
                    {
                new
                {
                    text = text
                }
            }
                }
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"https://generativelanguage.googleapis.com/v1beta/models/gemini-embedding-001:embedContent?key={apiKey}",
                


                request);

            response.EnsureSuccessStatusCode();

            var result =
                await response.Content
                    .ReadFromJsonAsync<GeminiEmbeddingResponse>();

            return new Vector(
                result.Embedding.Values.ToArray());
        }
    }
    //public class OllamaEmbeddingResponse
    //{
    //    public float[] embedding { get; set; }

    //}

    public class GeminiEmbeddingResponse
    {
        public GeminiEmbedding Embedding { get; set; }
    }
    public class GeminiEmbedding
    {
        public List<float> Values { get; set; }
    }
}
