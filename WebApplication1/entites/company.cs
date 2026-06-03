namespace WebApplication1.entites
{
    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; }

        // Geo Policy
        public string GeoAccessType { get; set; } // Global, Radius, Polygon

        public double? CenterLat { get; set; }
        public double? CenterLng { get; set; }
        public double? RadiusKm { get; set; }

        public string? Polygon { get; set; } // JSON (future use)
        public ICollection<Branch> ?Branches { get; set; }
        public ICollection<CompanyStateCountyAccess> ? CompanyStateCountyAccesses { get; set; }
        public ICollection<ClientCompanyBranchAccess>
          ClientCompanyBranchAccesses
        { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
