using System.Diagnostics.Metrics;

namespace WebApplication1.entites
{
    public class BranchStateCountyAccess
    {
        public int Id { get; set; }

        // Branch FK
        public int BranchId { get; set; }

        public Branch Branch { get; set; }

        // County FK
        public int CountyId { get; set; }

        public County County { get; set; }

        public bool IsDisabled { get; set; } = false;

        public DateTime CreatedDate { get; set; }
            = DateTime.UtcNow;
    }
}
