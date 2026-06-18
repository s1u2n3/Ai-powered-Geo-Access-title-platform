namespace WebApplication1.Dto
{
    public class ParcelResponseDto
    {

        public int Id { get; set; }

        public string ParcelId { get; set; }

        public string APN { get; set; }

        public string Address { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public object Polygon { get; set; }

    }
}
