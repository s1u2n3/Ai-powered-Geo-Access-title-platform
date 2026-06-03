namespace WebApplication1.Dto.Company
{
    public class CreateCompanyDto
    {
        public string Name { get; set; }
        
        public string GeoAccessType { get; set; }

        public double? CenterLat { get; set; }
        public double? CenterLng { get; set; }
        public double? RadiusKm { get; set; }

        public string? Polygon { get; set; }

        public List<CompanyGeoAccessDto> Locations { get; set; } 
    }
}
