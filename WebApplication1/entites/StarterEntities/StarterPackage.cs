namespace WebApplication1.entites.StarterEntities
{
    public class StarterPackage
    {
        public int Id { get; set; }

        public string ParcelId { get; set; }

        public int CoverageFromYear { get; set; }

        public int CoverageToYear { get; set; }

        public string StarterType { get; set; }

        public string Status { get; set; }

        public int ? RiskScore { get; set; }

        public string ? RiskLevel { get; set; }

        public string CurrentOwner { get; set; }

        public bool ? InsuranceReady { get; set; }

        public DateTime CreatedDate { get; set; }

        public string ? StarterJson { get; set; }
    }
}
