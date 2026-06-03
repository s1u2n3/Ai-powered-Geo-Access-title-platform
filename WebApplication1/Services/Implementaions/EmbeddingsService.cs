
using System.Net.Http.Json;
using Pgvector;

namespace WebApplication1.Services.Implementaions
{


    public class EmbeddingsService
    {
        private readonly HttpClient _httpClient;
        public EmbeddingsService(HttpClient httpClient)
        {

            _httpClient= httpClient; 
        }


        public  async Task<Vector> GenerateEmbeddingAsync(string text)
        {
            var request = new
            {
                model = "nomic-embed-text",
                prompt = text
            };

            var response =
                await _httpClient
                    .PostAsJsonAsync(
                        "http://localhost:11434/api/embeddings",
                        request);

            response
                .EnsureSuccessStatusCode();

            var result =
                await response.Content
                    .ReadFromJsonAsync<
                        OllamaEmbeddingResponse>();

            return new Vector(
                result.embedding);
        
    }
    }
    public class OllamaEmbeddingResponse
    {
        public float[] embedding { get; set; }
       
    }
}
