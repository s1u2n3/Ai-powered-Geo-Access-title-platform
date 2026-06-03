using System.Text.Json.Serialization;

namespace WebApplication1.SeedDto_s
{
    public class StateSeed
    {
        [JsonPropertyName("stateCode")]
        public string StateCode { get; set; }

        [JsonPropertyName("stateName")]
        public string StateName { get; set; }

        [JsonPropertyName("countryCode")]
        public string CountryCode { get; set; }

        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        [JsonPropertyName("counties")]
        public List<CountySeed> Counties { get; set; }
    }
}

