using System.Text.Json.Serialization;

namespace WebApplication1.SeedDto_s
{
    public class CountySeed
    {
        [JsonPropertyName("countyCode")]
        public string CountyCode { get; set; }

        [JsonPropertyName("countyName")]
        public string CountyName { get; set; }

        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }
    }
    
}
