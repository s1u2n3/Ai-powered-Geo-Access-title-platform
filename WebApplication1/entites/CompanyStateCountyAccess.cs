namespace WebApplication1.entites
{
    public class CompanyStateCountyAccess
    {
        public int Id { get; set; }

        // Company FK
        public int CompanyId { get; set; }

        public Company Company { get; set; }

        // County FK
        public int CountyId { get; set; }

        public County County { get; set; }

        public bool IsDisabled { get; set; } = false;

        public DateTime CreatedDate { get; set; }
            = DateTime.UtcNow;
    }
}
